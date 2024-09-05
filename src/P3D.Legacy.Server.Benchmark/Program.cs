using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Benchmark.Options;
using P3D.Legacy.Server.Benchmark.Services;
using P3D.Legacy.Server.Client.P3D;
using P3D.Legacy.Server.Client.P3D.Packets;

using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Benchmark;

public static class Program
{
    public static Task Main(string[] args) => new CommandLineBuilder(new RunBenchmarkCommand()).UseHost(static _ => new HostBuilder(), ConfigureHost).UseDefaults().Build().InvokeAsync(args);

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = new HostBuilder();
        ConfigureHost(builder);
        return builder;
    }

    private static void ConfigureHost(IHostBuilder hostBuilder) => hostBuilder
        .ConfigureServices(static (ctx, services) =>
        {
            services.Configure<ConsoleLifetimeOptions>(static opt => opt.SuppressStatusMessages = true);

            services.AddTransient<BenchmarkService>();
            services.AddTransient<BenchmarkStatusService>();

            services.AddSingleton<IP3DPacketFactory, P3DPacketServerFactory>();
            services.AddSingleton<P3DProtocol>();

            services.AddSingleton<P3DClientConnectionService>();

            services.AddOptions<CLIOptions>().Configure<BindingContext>(static (opt, bindingContext) => new ModelBinder<CLIOptions>().UpdateInstance(opt, bindingContext));

        }).ConfigureLogging(static (ctx, builder) =>
        {
            builder.ClearProviders();
            builder.AddSimpleConsole(static opt =>
            {
                opt.IncludeScopes = true;
                opt.SingleLine = true;
                opt.TimestampFormat = "HH:mm:ss:fff ";
            });
            builder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        }).UseCommandHandler<RunBenchmarkCommand, RunBenchmarkCommand.Handler>();
}