using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Services;

using System.Net;
using Bedrock.Framework;
using Microsoft.Extensions.Configuration;
using P3D.Legacy.Server.Options;

namespace P3D.Legacy.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.Configure<ServerOptions>(ctx.Configuration.GetSection("Server"));

                services.AddTransient<P3DPacketFactory>();
                services.AddTransient<P3DProtocol>();
                services.AddSingleton<WorldService>();
                services.AddSingleton<PlayerHandlerService>();
            })
            /*
            .ConfigureServer(server =>
            {
                server.UseSockets(sockets =>
                {
                    sockets.ListenLocalhost(15124, builder =>
                    {
                        builder.UseConnectionLogging().UseConnectionHandler<P3DConnectionHandler>();
                    });
                });
            })
            */
            .ConfigureWebHostDefaults(webBuilder => webBuilder
                .UseKestrel((ctx, options) =>
                {
                    var serverOptions = ctx.Configuration.GetSection("Server").Get<ServerOptions>();
                    options.Listen(new IPEndPoint(IPAddress.Parse(serverOptions.IP), serverOptions.Port), builder =>
                    {
                        builder.UseConnectionHandler<P3DConnectionHandler>();
                    });
                })
                .UseStartup<Startup>()
            )

        ;
    }
}
