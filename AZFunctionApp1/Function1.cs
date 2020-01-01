using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using NewRelic.Api.Agent;

namespace AZFunctionApp1
{
    public static class Function1
    {
        //static Function1()
        //{
        //    NewRelic.Api.Agent.NewRelic.StartAgent();
        //}

//        [Transaction]
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //IAgent agent = NewRelic.Api.Agent.NewRelic.GetAgent();
            //ITransaction transaction = agent?.CurrentTransaction;

            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;


            var appName = Environment.GetEnvironmentVariable("NEW_RELIC_APP_NAME");
            var pwd = Environment.CurrentDirectory;
            var loc = System.Reflection.Assembly.GetAssembly(typeof(Function1)).Location;
            var isD = Directory.Exists(@"D:\home\site\wwwroot\newrelic");
            var isF = File.Exists(@"D:\home\site\wwwroot\newrelic\NewRelic.Profiler.dll");
            
            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}. {appName},{pwd},{loc},{isD},{isF}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
