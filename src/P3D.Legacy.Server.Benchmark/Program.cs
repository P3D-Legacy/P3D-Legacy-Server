using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Benchmark.Services;

using System;
using System.Net;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Benchmark
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var network1 = IPNetwork.Parse(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("255.255.0.0"));
            var network2 = IPNetwork.Parse(IPAddress.Parse("212.1.1.1"), IPAddress.Parse("255.255.0.0"));
            var network3 = IPNetwork.Parse(IPAddress.Parse("212.1.1.2"), IPAddress.Parse("255.255.0.0"));
            var network4 = IPNetwork.Parse(IPAddress.Parse("23.1.1.1"), IPAddress.Parse("255.255.0.0"));
            var network5 = IPNetwork.Parse(IPAddress.Parse("23.1.1.2"), IPAddress.Parse("255.255.0.0"));


            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);

                services.AddHostedService<BenchmarkService>();
                //services.AddHostedService<BenchmarkStatusService>();

                services.AddSingleton<P3DClientConnectionService>();
            })
        ;
    }
}
