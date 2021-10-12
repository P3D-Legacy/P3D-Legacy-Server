using Bedrock.Framework;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Extensions;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Services;

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

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
                services.Configure<P3DOptions>(ctx.Configuration.GetSection("P3D"));


                services.AddHttpClient("P3D.API")
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var backendOptions = sp.GetRequiredService<IOptions<P3DOptions>>().Value;
                        client.BaseAddress = new Uri(backendOptions.APIEndpoint);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", backendOptions.APIToken);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    })
                    .GenerateCorrelationId()
                    .AddPolly()
                    .AddCorrelationIdOverrideForwarding();

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