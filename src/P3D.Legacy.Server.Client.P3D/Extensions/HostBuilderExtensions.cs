using Bedrock.Framework;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Client.P3D.Options;

using System.Net;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddP3DServer(this IHostBuilder hostBuilder) => hostBuilder.ConfigureServer((ctx, server) =>
        {
            var serverOptions = server.ApplicationServices.GetRequiredService<IOptions<P3DServerOptions>>().Value;
            server.UseSockets(sockets =>
            {
                sockets.Listen(new IPEndPoint(IPAddress.Parse(serverOptions.IP), serverOptions.Port), builder =>
                {
                    builder.UseConnectionLogging().UseConnectionHandler<P3DConnectionHandler>();
                });
            });
        });
    }
}