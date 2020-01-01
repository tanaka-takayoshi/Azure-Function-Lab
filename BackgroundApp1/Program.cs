using Microsoft.Extensions.Hosting;
using System;

namespace BackgroundApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
            });
            var host = builder.Build();
            using (host)
            {
                host.Run();
            }
        }
    }
}
