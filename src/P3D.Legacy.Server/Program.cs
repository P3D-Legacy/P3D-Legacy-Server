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

                services.AddOpenTelemetry()
                    .ConfigureResource(static builder => builder.AddService(typeof(Program).Namespace!))
                    .WithMetrics(builder =>
                    {
                        var enabled = ctx.Configuration.GetSection("Otlp").GetValue<bool>(nameof(OtlpOptions.Enabled));
                        var host = ctx.Configuration.GetSection("Otlp").GetValue<string>(nameof(OtlpOptions.Host)) ?? string.Empty;
                        if (enabled)
                        {
                            builder.AddAspNetCoreInstrumentation();
                            builder.AddHttpClientInstrumentation();

                            builder.AddStatisticsInstrumentation();

                            builder.AddOtlpExporter(opt =>
                            {
                                opt.Endpoint = new Uri(host);
                            });
                        }
                    })
                    .WithTracing(builder =>
                    {
                        var enabled = ctx.Configuration.GetSection("Otlp").GetValue<bool>(nameof(OtlpOptions.Enabled));
                        var host = ctx.Configuration.GetSection("Otlp").GetValue<string>(nameof(OtlpOptions.Host)) ?? string.Empty;
                        if (enabled)
                        {
                            builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("P3D.Legacy.Server"));

                            builder.AddAspNetCoreInstrumentation();
                            builder.AddHttpClientInstrumentation();

                            builder.AddHostInstrumentation();
                            builder.AddApplicationInstrumentation();
                            builder.AddCQERSInstrumentation();
                            builder.AddClientP3DInstrumentation();
                            builder.AddCommunicationAPIInstrumentation();
                            builder.AddDiscordBotInstrumentation();
                            builder.AddGameCommandsInstrumentation();
                            builder.AddInfrastructureInstrumentation();
                            builder.AddInternalAPIInstrumentation();
                            builder.AddStatisticsInstrumentation();

                            builder.AddOtlpExporter(opt =>
                            {
                                opt.Endpoint = new Uri(host);
                            });
                        }
                    });
            })
            .AddP3DServer()
            .ConfigureWebHostDefaults(static webBuilder => webBuilder.UseStartup<Startup>())
        ;
    }
}