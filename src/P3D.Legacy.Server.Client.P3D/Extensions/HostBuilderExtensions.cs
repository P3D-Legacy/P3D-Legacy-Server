using Bedrock.Framework;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Client.P3D.Options;

using System.Net;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddP3DServer(this IHostBuilder hostBuilder) => hostBuilder.ConfigureServer((ctx, server) =>
        {
            server.UseSockets(sockets =>
            {
                var serverOptions = ctx.Configuration.GetSection("P3DServer").Get<P3DServerOptions>();
                sockets.Listen(new IPEndPoint(IPAddress.Parse(serverOptions.IP), serverOptions.Port), builder =>
                {
                    builder.UseConnectionLogging().UseConnectionHandler<P3DConnectionHandler>();
                });
            });
        });
    }
}