using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Exporter.NewRelic;
using OpenTelemetry.Trace.Sampler;
using OpenTelemetry.Trace;
using System.Collections.Generic;

namespace AZFunctionOpenTelemetry
{
    public static class Function1
    {
        private static object lockObject = new object();
        private static bool isColdStart = true;

        public static ITracer OTTracer;

        static Function1()
        {
            // Obtain the API Key from the Web.Config file
            var apiKey = Environment.GetEnvironmentVariable("NEWRELIC_TELEMETRY_APIKEY");

            // Create the tracer factory registering New Relic as the Data Exporter
            var tracerFactory = TracerFactory.Create((b) =>
            {
                b.UseNewRelic(apiKey)
                .SetSampler(Samplers.AlwaysSample);
            });

            // Make the tracer available to the application
            OTTracer = tracerFactory.GetTracer("AZFunctionOpenTelemetry");
        }

        [FunctionName("Function2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var context = OTTracer.TextFormat.Extract(req.Headers, (headers, name) => headers[name]);
            var incomingSpan = OTTracer.StartSpan("Function2", context, SpanKind.Server);
            incomingSpan.SetAttribute("service.name", "AZFunctionOpenTelemetry");

            lock (lockObject)
            {
                if (isColdStart)
                {
                    isColdStart = false;
                    incomingSpan.SetAttribute("azure.function.iscoldstart", "true");
                }
                else
                {
                    incomingSpan.SetAttribute("azure.function.iscoldstart", "false");
                }
            }

            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                string name = req.Query["name"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                name = name ?? data?.name;

                return name != null
                    ? (ActionResult)new OkObjectResult($"Hello, {name}")
                    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }
            // If an unhandled exception occurs, it can be denoted on the span.
            catch (Exception ex)
            {
                incomingSpan.Status = Status.Internal;
                incomingSpan.SetAttribute(KeyValuePair.Create<string, object>("exception", ex));
                throw;
            }
            // In all cases, the span is sent up to the New Relic endpoint.
            finally
            {
                incomingSpan.End();
            }
        }
    }
}
