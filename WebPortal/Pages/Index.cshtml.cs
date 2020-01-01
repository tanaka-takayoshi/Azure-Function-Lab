using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using Newtonsoft.Json;

namespace WebPortal.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string Name { get; set; }

        public IndexModel()
        {
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //var connectionString = configuration.GetConnectionString("AzureStorage"); 
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=msgdt;AccountKey=S2Wdi/GXFhmG0eneS8NeS3l/K36JNkz8ZvJF052DuN639bqxiSVqF29q/PnlsHHb6bUaWhQsg+ZiEpbZxrexQA==;EndpointSuffix=core.windows.net");
            //var storageAccount = CloudStorageAccount.Parse(
            //    CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference("queue");

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();

            IAgent agent = NewRelic.Api.Agent.NewRelic.GetAgent();
            ITransaction transaction = agent.CurrentTransaction;
            IDistributedTracePayload payload = transaction.CreateDistributedTracePayload();
            var pl = JsonConvert.DeserializeObject(payload.Text());
            var obj = new { name=Name,payload= pl};

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(obj));
            queue.AddMessage(message);

            return Page();
        }


    }
}
