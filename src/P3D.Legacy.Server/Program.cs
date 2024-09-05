using Aragas.Extensions.Options.FluentValidation.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.ResourceDetectors.Container;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using P3D.Legacy.Server.Abstractions.Configuration;
using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Client.P3D.Extensions;
using P3D.Legacy.Server.Client.P3D.Options;
using P3D.Legacy.Server.CommunicationAPI.Extensions;
using P3D.Legacy.Server.CQERS.Extensions;
using P3D.Legacy.Server.DiscordBot.Extensions;
using P3D.Legacy.Server.DiscordBot.Options;
using P3D.Legacy.Server.Extensions;
using P3D.Legacy.Server.GameCommands.Extensions;
using P3D.Legacy.Server.GUI.Extensions;
using P3D.Legacy.Server.Infrastructure.Extensions;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.InternalAPI.Extensions;
using P3D.Legacy.Server.InternalAPI.Options;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Statistics.Extensions;

using Serilog;
using Serilog.Events;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace P3D.Legacy.Server;

[RequiresUnreferencedCode("Configuration and OpenTelemetry")]
public static class Program
{
    private const string ServerSectionName = "Server";
    private const string LiteDbSectionName = "LiteDb";
    private const string OfficialSiteSectionName = "OfficialSite";
    private const string P3DServerSectionName = "P3DServer";
    private const string DiscordBotSectionName = "DiscordBot";
    private const string JwtSectionName = "Jwt";
    private const string OtlpSectionName = "Otlp";

    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting web application");

            var host = CreateHostBuilder(args).Build();

            await host
                .RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
        .UseConsoleLifetime(static opt => opt.SuppressStatusMessages = false)
        .ConfigureAppConfiguration(static (ctx, builder) =>
        {
            builder.Add(new MemoryConfigurationProvider<ServerOptions>(ctx.Configuration.GetSection(ServerSectionName)));
            builder.Add(new MemoryConfigurationProvider<LiteDbOptions>(ctx.Configuration.GetSection(LiteDbSectionName)));
        })
        .ConfigureServices(static (ctx, services) =>
        {
            services.AddSingleton<DynamicConfigurationProviderManager>();

            services.AddValidatedOptions<ServerOptions, ServerOptionsValidator>().Bind(ctx.Configuration.GetSection(ServerSectionName));
            services.AddValidatedOptions<P3DIntegrationOptions, P3DIntegrationOptionsValidator>().Bind(ctx.Configuration.GetSection(ServerSectionName));
            services.AddValidatedOptionsWithHttp<P3DSiteOptions, P3DSiteOptionsValidator>().Bind(ctx.Configuration.GetSection(OfficialSiteSectionName));
            services.AddValidatedOptions<P3DServerOptions, P3DServerOptionsValidator>().Bind(ctx.Configuration.GetSection(P3DServerSectionName));
            services.AddValidatedOptions<DiscordOptions, DiscordOptionsValidator>().Bind(ctx.Configuration.GetSection(DiscordBotSectionName));
            services.AddValidatedOptions<LiteDbOptions, LiteDbOptionsValidator>().Bind(ctx.Configuration.GetSection(LiteDbSectionName));
            services.AddValidatedOptions<JwtOptions, JwtOptionsValidator>().Bind(ctx.Configuration.GetSection(JwtSectionName));
            services.AddValidatedOptions<OtlpOptions, OtlpOptionsValidator>().Bind(ctx.Configuration.GetSection(OtlpSectionName));

            services.AddMediator();
            services.AddHost();
            services.AddApplication();
            services.AddClientP3D();
            services.AddCommunicationAPI();
            services.AddDiscordBot();
            services.AddGameCommands();
            services.AddInfrastructure();
            services.AddInternalAPI();
            services.AddStatistics();
            services.AddGUI();

            // Sorry (not sorry) for this hack
            // Will show the application name in OpenObserve UI as show in the P3D Client
            if (services.FirstOrDefault(sp => typeof(IHostEnvironment).IsAssignableFrom(sp.ServiceType))?.ImplementationInstance is IHostEnvironment he)
            {
                var serverName = ctx.Configuration.GetSection(ServerSectionName).GetValue<string>(nameof(ServerOptions.Name))
                                 ?? typeof(Program).Assembly.GetName().Name
                                 ?? "P3D.Legacy.Server";
                he.ApplicationName = serverName;
            }
        })
        .ConfigureServices((ctx, services) =>
        {
            var openTelemetry = services.AddOpenTelemetry()
                .WithMetrics()
                .WithTracing()
                .WithLogging();

            if (ctx.Configuration.GetSection(OtlpSectionName) is { } oltpSection)
            {
                openTelemetry
                    .ConfigureResource(builder =>
                    {
                        builder.AddDetector(new ContainerResourceDetector());
                        builder.AddService(
                            ctx.HostingEnvironment.ApplicationName,
                            ctx.HostingEnvironment.EnvironmentName,
                            typeof(Program).Assembly.GetName().Version?.ToString(),
                            false,
                            Environment.MachineName);
                        builder.AddTelemetrySdk();
                    });

                if (oltpSection.GetValue<string?>(nameof(OtlpOptions.MetricsEndpoint)) is { } metricsEndpoint)
                {
                    var metricsProtocol = oltpSection.GetValue<OtlpExportProtocol>(nameof(OtlpOptions.MetricsProtocol));
                    openTelemetry.WithMetrics(builder => builder
                        .AddProcessInstrumentation()
                        .AddRuntimeInstrumentation(instrumentationOptions =>
                        {

                        })
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddStatisticsInstrumentation()
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(metricsEndpoint);
                            o.Protocol = metricsProtocol;
                        }));
                }

                if (oltpSection.GetValue<string?>(nameof(OtlpOptions.TracingEndpoint)) is { } tracingEndpoint)
                {
                    var tracingProtocol = oltpSection.GetValue<OtlpExportProtocol>(nameof(OtlpOptions.TracingProtocol));
                    openTelemetry.WithTracing(builder => builder
                        .AddGrpcClientInstrumentation(instrumentationOptions =>
                        {

                        })
                        .AddHttpClientInstrumentation(instrumentationOptions =>
                        {
                            instrumentationOptions.RecordException = true;
                        })
                        .AddAspNetCoreInstrumentation(instrumentationOptions =>
                        {
                            instrumentationOptions.RecordException = true;
                        })
                        .AddHostInstrumentation()
                        .AddApplicationInstrumentation()
                        .AddCQERSInstrumentation()
                        .AddClientP3DInstrumentation()
                        .AddCommunicationAPIInstrumentation()
                        .AddDiscordBotInstrumentation()
                        .AddGameCommandsInstrumentation()
                        .AddInfrastructureInstrumentation()
                        .AddInternalAPIInstrumentation()
                        .AddStatisticsInstrumentation()
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(tracingEndpoint);
                            o.Protocol = tracingProtocol;
                        }));
                }
            }
        })
        .ConfigureLogging((ctx, builder) =>
        {
            var oltpSection = ctx.Configuration.GetSection(OtlpSectionName);
            if (oltpSection == null!) return;

            var loggingEndpoint = oltpSection.GetValue<string>(nameof(OtlpOptions.LoggingEndpoint));
            if (loggingEndpoint is null) return;
            var loggingProtocol = oltpSection.GetValue<OtlpExportProtocol>(nameof(OtlpOptions.LoggingProtocol));

            builder.AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.ParseStateValues = true;
                o.IncludeFormattedMessage = true;
                o.AddOtlpExporter((options, processorOptions) =>
                {
                    options.Endpoint = new Uri(loggingEndpoint);
                    options.Protocol = loggingProtocol;
                });
            });
        })
        .AddP3DServer()
        .ConfigureWebHostDefaults(static webBuilder => webBuilder.UseStartup<Startup>())
    ;
}