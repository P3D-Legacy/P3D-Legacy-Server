using Bedrock.Framework;

using BetterHostedServices;

using CorrelationId;
using CorrelationId.DependencyInjection;

using Discord;
using Discord.WebSocket;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Application.Queries.Bans;
using P3D.Legacy.Server.Application.Queries.Permissions;
using P3D.Legacy.Server.Application.Queries.Players;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Application.Services.Connections;
using P3D.Legacy.Server.BackgroundServices;
using P3D.Legacy.Server.Extensions;
using P3D.Legacy.Server.GameCommands.Extensions;
using P3D.Legacy.Server.Infrastructure.Repositories;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Services;
using P3D.Legacy.Server.Services.Server;
using P3D.Legacy.Server.Utils.HttpLogging;

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.Configure<ServerOptions>(ctx.Configuration.GetSection("Server"));
                services.Configure<P3DOptions>(ctx.Configuration.GetSection("P3D"));
                services.Configure<DiscordOptions>(ctx.Configuration.GetSection("Discord"));

                services.AddGameCommands();

                services.AddBetterHostedServices();

                services.AddMediatRInternal(
                    GameCommands.Extensions.ServiceCollectionExtensions.AddGameCommandsNotifications()
                    );

                services.AddSingleton<DefaultJsonSerializer>();

                services.AddDefaultCorrelationId(options =>
                {
                    options.AddToLoggingScope = true;
                    options.EnforceHeader = true;
                    options.IgnoreRequestHeader = false;
                    options.IncludeInResponse = true;
                    options.RequestHeader = CorrelationIdOptions.DefaultHeader;
                    options.ResponseHeader = CorrelationIdOptions.DefaultHeader;
                    options.UpdateTraceIdentifier = false;
                });

                services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
                services.AddSingleton<IHttpMessageHandlerBuilderFilter, LoggingHttpMessageHandlerBuilderFilter>();

                services.AddHttpClient("P3D.API")
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var backendOptions = sp.GetRequiredService<IOptions<P3DOptions>>().Value;
                        client.BaseAddress = new Uri(backendOptions.APIEndpointV1);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", backendOptions.APIToken);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    })
                    .GenerateCorrelationId()
                    .AddPolly()
                    .AddCorrelationIdOverrideForwarding();

                services.AddSingleton<P3DPacketFactory>();

                services.AddScoped<P3DConnectionContextHandler>();

                services.AddScoped<ConnectionContextHandlerFactory>();
                services.AddScoped<P3DProtocol>();

                services.AddSingleton<IPlayerIdGenerator, DefaultPlayerIdGenerator>();

                services.AddTransient<IBanQueries, DefaultBanQueries>();
                services.AddTransient<IPermissionQueries, P3DAPIPermissionQueries>();
                services.AddTransient<IPlayerQueries, PlayerQueries>();

                services.AddTransient<IBanRepository, BanRepository>();

                services.AddSingleton<DefaultPlayerContainer>();
                services.AddTransient<IPlayerContainerWriter>(sp => sp.GetRequiredService<DefaultPlayerContainer>());
                services.AddTransient<IPlayerContainerReader>(sp => sp.GetRequiredService<DefaultPlayerContainer>());
                services.AddTransient<IPlayerContainerActions>(sp => sp.GetRequiredService<DefaultPlayerContainer>());

                services.AddHostedServiceAsSingleton<WorldService>();

                services.AddHostedServiceAsSingleton<DiscordPassthroughService>();
                services.AddSingleton<DiscordSocketClient>();
                services.AddSingleton<IDiscordClient, DiscordSocketClient>(sp => sp.GetRequiredService<DiscordSocketClient>());
            })
            .ConfigureServer((ctx, server) =>
            {
                server.UseSockets(sockets =>
                {
                    var serverOptions = ctx.Configuration.GetSection("Server").Get<ServerOptions>();
                    sockets.Listen(new IPEndPoint(IPAddress.Parse(serverOptions.IP), serverOptions.Port), builder =>
                    {
                        builder.UseConnectionLogging().UseConnectionHandler<P3DConnectionHandler>();
                    });
                });
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder
                /*
                .UseKestrel((ctx, options) =>
                {
                    var serverOptions = ctx.Configuration.GetSection("Server").Get<ServerOptions>();
                    options.Listen(new IPEndPoint(IPAddress.Parse(serverOptions.IP), serverOptions.Port), builder =>
                    {
                        builder.UseConnectionHandler<P3DConnectionHandler>();
                    });
                })
                */
                .UseStartup<Startup>()
            )

        ;
    }
}