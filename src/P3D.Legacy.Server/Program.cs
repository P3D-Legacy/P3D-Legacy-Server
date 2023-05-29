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
using System.Diagnostics;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public static class Program
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
            .UseConsoleLifetime(static opt => opt.SuppressStatusMessages = false)
            .ConfigureAppConfiguration(static (ctx, builder) =>
            {
                builder.Add(new MemoryConfigurationProvider<ServerOptions>(ctx.Configuration.GetSection("Server")));
                builder.Add(new MemoryConfigurationProvider<LiteDbOptions>(ctx.Configuration.GetSection("LiteDb")));
            })
            .ConfigureServices(static (ctx, services) =>
            {
                services.AddSingleton<DynamicConfigurationProviderManager>();

                services.AddValidatedOptions<ServerOptions, ServerOptionsValidator>(ctx.Configuration.GetSection("Server"));
                services.AddValidatedOptions<P3DIntegrationOptions, P3DIntegrationOptionsValidator>(ctx.Configuration.GetSection("Server"));
                services.AddValidatedOptionsWithHttp<P3DSiteOptions, P3DSiteOptionsValidator>(ctx.Configuration.GetSection("OfficialSite"));
                services.AddValidatedOptions<P3DServerOptions, P3DServerOptionsValidator>(ctx.Configuration.GetSection("P3DServer"));
                services.AddValidatedOptions<DiscordOptions, DiscordOptionsValidator>(ctx.Configuration.GetSection("DiscordBot"));
                services.AddValidatedOptions<LiteDbOptions, LiteDbOptionsValidator>(ctx.Configuration.GetSection("LiteDb"));
                services.AddValidatedOptions<JwtOptions, JwtOptionsValidator>(ctx.Configuration.GetSection("Jwt"));
                services.AddValidatedOptions<OtlpOptions, OtlpOptionsValidator>(ctx.Configuration.GetSection("Otlp"));

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

                services.AddOpenTelemetry()
                    .ConfigureResource(static builder => builder.AddService("P3D.Legacy.Server"))
                    .WithMetrics(builder =>
                    {
                        var options = ctx.Configuration.GetSection("Otlp").Get<OtlpOptions>() ?? new();
                        if (options.Enabled)
                        {
                            builder.AddAspNetCoreInstrumentation();
                            builder.AddHttpClientInstrumentation();

                            builder.AddStatisticsInstrumentation();

                            builder.AddOtlpExporter(opt =>
                            {
                                opt.Endpoint = new Uri(options.Host);
                            });
                        }
                    })
                    .WithTracing(builder =>
                    {
                        var options = ctx.Configuration.GetSection("Otlp").Get<OtlpOptions>() ?? new();
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

                            builder.AddOtlpExporter(opt =>
                            {
                                opt.Endpoint = new Uri(options.Host);
                            });
                        }
                    });
            })
            .AddP3DServer()
            .ConfigureWebHostDefaults(static webBuilder => webBuilder.UseStartup<Startup>())
        ;
    }
}