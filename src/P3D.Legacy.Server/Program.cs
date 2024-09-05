using Aragas.Extensions.Options.FluentValidation.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Metrics;
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using OpenTelemetry.Exporter;
using OpenTelemetry.ResourceDetectors.Container;

namespace P3D.Legacy.Server
{
    [RequiresUnreferencedCode("Configuration and OpenTelemetry")]
    public static class Program
    {
        public static Task Main(string[] args) => CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .UseConsoleLifetime(static opt => opt.SuppressStatusMessages = false)
            .ConfigureAppConfiguration(static (ctx, builder) =>
            {
                builder.Add(new MemoryConfigurationProvider<ServerOptions>(ctx.Configuration.GetSection("Server")));
                builder.Add(new MemoryConfigurationProvider<LiteDbOptions>(ctx.Configuration.GetSection("LiteDb")));
            })
            .ConfigureServices(static (ctx, services) =>
            {
                services.AddSingleton<DynamicConfigurationProviderManager>();

                services.AddValidatedOptions<ServerOptions, ServerOptionsValidator>().Bind(ctx.Configuration.GetSection("Server"));
                services.AddValidatedOptions<P3DIntegrationOptions, P3DIntegrationOptionsValidator>().Bind(ctx.Configuration.GetSection("Server"));
                services.AddValidatedOptionsWithHttp<P3DSiteOptions, P3DSiteOptionsValidator>().Bind(ctx.Configuration.GetSection("OfficialSite"));
                services.AddValidatedOptions<P3DServerOptions, P3DServerOptionsValidator>().Bind(ctx.Configuration.GetSection("P3DServer"));
                services.AddValidatedOptions<DiscordOptions, DiscordOptionsValidator>().Bind(ctx.Configuration.GetSection("DiscordBot"));
                services.AddValidatedOptions<LiteDbOptions, LiteDbOptionsValidator>().Bind(ctx.Configuration.GetSection("LiteDb"));
                services.AddValidatedOptions<JwtOptions, JwtOptionsValidator>().Bind(ctx.Configuration.GetSection("Jwt"));
                services.AddValidatedOptions<OtlpOptions, OtlpOptionsValidator>().Bind(ctx.Configuration.GetSection("Otlp"));

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
            })
           .ConfigureServices((ctx, services) =>
            {
                if (ctx.Configuration.GetSection("Oltp") is { } oltpSection)
                {
                    var openTelemetry = services.AddOpenTelemetry()
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

                    if (oltpSection.GetValue<string?>("MetricsEndpoint") is { } metricsEndpoint)
                    {
                        var metricsProtocol = oltpSection.GetValue<OtlpExportProtocol>("MetricsProtocol");
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

                    if (oltpSection.GetValue<string?>("TracingEndpoint") is { } tracingEndpoint)
                    {
                        var tracingProtocol = oltpSection.GetValue<OtlpExportProtocol>("TracingProtocol");
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
            .AddP3DServer()
            .ConfigureWebHostDefaults(static webBuilder => webBuilder.UseStartup<Startup>())
        ;
    }
}