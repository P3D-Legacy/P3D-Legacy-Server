using Bedrock.Framework;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.Utils;
using P3D.Legacy.Server.DiscordAPI.Extensions;
using P3D.Legacy.Server.DiscordBot.Extensions;
using P3D.Legacy.Server.Extensions;
using P3D.Legacy.Server.GameCommands.Extensions;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Services;
using P3D.Legacy.Server.Statistics.Extensions;

using System;
using System.Diagnostics;
using System.Net;
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
                var useDiscordBot = ctx.Configuration["Server:UseDiscordBot"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

                var requestRegistrar = new RequestRegistrar();
                var notificationRegistrar = new NotificationRegistrar();
                services.AddInternal(ctx.Configuration, requestRegistrar, notificationRegistrar);
                services.AddGameCommands(ctx.Configuration, requestRegistrar, notificationRegistrar);
                services.AddStatistics(ctx.Configuration, requestRegistrar, notificationRegistrar);
                if (useDiscordBot) services.AddDiscordBot(ctx.Configuration, requestRegistrar, notificationRegistrar);
                services.AddDiscordAPI(ctx.Configuration, requestRegistrar, notificationRegistrar);
                services.AddMediatRInternal(requestRegistrar, notificationRegistrar);
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