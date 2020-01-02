using OpenTelemetry.Exporter.NewRelic;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Sampler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace AZFunctionOpenTelemetryInvoker
{
    class Program
    {
        public static ITracer OTTracer;

        static Program()
        {
            // Obtain the API Key from the Web.Config file
            var apiKey = "NRII-6QcNiBTmxnHt7py-dq3IR3snWdyiw0lK";// Environment.GetEnvironmentVariable("NEWRELIC_TELEMETRY_APIKEY");

            // Create the tracer factory registering New Relic as the Data Exporter
            var tracerFactory = TracerFactory.Create((b) =>
            {
                b.UseNewRelic(apiKey)
                .SetSampler(Samplers.AlwaysSample)
                .AddDependencyCollector();
                //.SetResource(new Resource(new Dictionary<string, string>() { { "service.name", "my-service" } }))
            });

            // Make the tracer available to the application
            OTTracer = tracerFactory.GetTracer("AZFunctionOpenTelemetryInvoker");
        }

        static void Main(string[] args)
        {
            //var outgoingRequest = new HttpRequestMessage(HttpMethod.Get, "https://azfunctionopentelemetry20200102023740.azurewebsites.net/api/Function2?name=hoge");
            var outgoingSpan = OTTracer.StartRootSpan("outgoing http request", SpanKind.Client);
            //.SetResource(new Resource(new Dictionary<string, string>() { { "service.name", "my-service" } }))
            outgoingSpan.SetAttribute("service.name", "AZFunctionOpenTelemetryInvoker");
            var client = new HttpClient();

            // now that we have outgoing span, we can inject it's context
            // Note that if there is no SDK configured, tracer is noop -
            // it creates noop spans with invalid context. we should not propagate it.
            if (outgoingSpan.Context.IsValid)
            {
                OTTracer.TextFormat.Inject(
                    outgoingSpan.Context,
                    client.DefaultRequestHeaders,
                    (headers, name, value) => headers.Add(name, value));
            }

            var res = client.GetAsync("https://azfunctionopentelemetry20200102023740.azurewebsites.net/api/Function2?name=hoge")
                .GetAwaiter().GetResult();
            Console.WriteLine(res.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            outgoingSpan.End();
            Console.ReadLine();
        }
    }
}
