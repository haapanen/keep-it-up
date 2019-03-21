using System;
using System.IO;
using System.Threading.Tasks;
using KeepItUp.Communication;
using KeepItUp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeepItUp.Service
{
    public class Program
    {
        public static void Main(string[] args) => new Program().RunAsync(args).Wait();

        private async Task RunAsync(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appSettings.json")
                    .Build();


            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(cfg =>
                {
                    cfg.SetBasePath(Directory.GetCurrentDirectory());
                    cfg.AddJsonFile("appSettings.json");
                })
                .ConfigureServices(cfg =>
                {
                    cfg.AddLogging();
                    cfg.AddHostedService<KeepItUpService>();
                    cfg.AddDbContext<KeepItUpContext>();
                    cfg.AddTransient<ETClient>();
                    cfg.AddSingleton(configuration);
                })
                .ConfigureLogging(cfg => { cfg.AddConsole(); });

            await hostBuilder.Build().StartAsync();
        }
    }
}
