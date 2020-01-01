using Microsoft.Azure.WebJobs;
using NewRelic.Api.Agent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundApp1
{
    public class Functions
    {
        [Transaction]
        public static void ProcessQueueMessage([QueueTrigger("queue")] string message)
        {
            //var typeExample = new { name = "", payload = ""};
            dynamic result = JsonConvert.DeserializeObject(message);
            Console.WriteLine($"triggered: {result.payload}");
            if (result?.payload != null)
            {
                Console.WriteLine($"payload applying: {JsonConvert.SerializeObject(result.payload)}");
                var agent = NewRelic.Api.Agent.NewRelic.GetAgent();
                var transaction = agent.CurrentTransaction;
                transaction.AcceptDistributedTracePayload(JsonConvert.SerializeObject(result.payload), TransportType.Queue);

            }
            Task.Delay(TimeSpan.FromSeconds(new Random().NextDouble()*5)).GetAwaiter().GetResult();
            Console.WriteLine("Executed");
        }
    }
}
