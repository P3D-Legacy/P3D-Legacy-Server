using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Client.P3D.Extensions;
using P3D.Legacy.Server.Client.P3D.Options;
using P3D.Legacy.Server.CommunicationAPI.Extensions;
using P3D.Legacy.Server.DiscordBot.Extensions;
using P3D.Legacy.Server.DiscordBot.Options;
using P3D.Legacy.Server.Extensions;
using P3D.Legacy.Server.GameCommands.Extensions;
using P3D.Legacy.Server.Infrastructure.Extensions;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.InternalAPI.Extensions;
using P3D.Legacy.Server.InternalAPI.Options;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Statistics.Extensions;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);

                services.Configure<ServerOptions>(ctx.Configuration.GetSection("Server"));
                services.Configure<P3DSiteOptions>(ctx.Configuration.GetSection("OfficialSite"));
                services.Configure<P3DServerOptions>(ctx.Configuration.GetSection("P3DServer"));
                services.Configure<DiscordOptions>(ctx.Configuration.GetSection("DiscordBot"));
                services.Configure<LiteDbOptions>(ctx.Configuration.GetSection("LiteDb"));
                services.Configure<JwtOptions>(ctx.Configuration.GetSection("Jwt"));

                var otlpSection = ctx.Configuration.GetSection("Otlp");

                services.AddMediatRInternal();
                using (var requestRegistrar = new RequestRegistrar(services))
                using (var notificationRegistrar = new NotificationRegistrar(services))
                {
                    services.AddHostMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddApplicationMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddClientP3DMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddCommunicationAPIMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddDiscordBotMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddGameCommandsMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddInfrastructureMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddInternalAPIMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddStatisticsMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                }

                services.AddHost(ctx.Configuration);
                services.AddApplication(ctx.Configuration);
                services.AddClientP3D(ctx.Configuration);
                services.AddCommunicationAPI(ctx.Configuration);
                services.AddDiscordBot(ctx.Configuration);
                services.AddGameCommands(ctx.Configuration);
                services.AddInfrastructure(ctx.Configuration);
                services.AddInternalAPI(ctx.Configuration);
                services.AddStatistics(ctx.Configuration);

                services.AddOpenTelemetryMetrics(builder =>
                {
                    var options = otlpSection.Get<OtlpOptions>();
                    if (options.Enabled)
                    {
                        builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("P3D.Legacy.Server"));

                        builder.AddAspNetCoreInstrumentation();
                        builder.AddHttpClientInstrumentation();

                        builder.AddStatisticsInstrumentation();

                        builder.AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(options.Host);
                        });
                    }
                });
                services.AddOpenTelemetryTracing(builder =>
                {
                    var options = otlpSection.Get<OtlpOptions>();
                    if (options.Enabled)
                    {
                        builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("P3D.Legacy.Server"));

                        builder.AddAspNetCoreInstrumentation();
                        builder.AddHttpClientInstrumentation();

                        builder.AddHostInstrumentation();
                        builder.AddApplicationInstrumentation();
                        builder.AddClientP3DInstrumentation();
                        builder.AddCommunicationAPIInstrumentation();
                        builder.AddDiscordBotInstrumentation();
                        builder.AddGameCommandsInstrumentation();
                        builder.AddInfrastructureInstrumentation();
                        builder.AddInternalAPIInstrumentation();
                        builder.AddStatisticsInstrumentation();

                        builder.AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(options.Host);
                        });
                    }
                });
            })
            .AddP3DServer()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .ConfigureLogging((ctx, builder) =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.IncludeScopes = true;
                    options.ParseStateValues = true;
                    options.IncludeFormattedMessage = true;
                    //options.AddConsoleExporter();
                });
            })
        ;
    }
}