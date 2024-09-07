using Bedrock.Framework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using P3D.Legacy.Server.Connections.Utils;

using System;

namespace P3D.Legacy.Server.Connections.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureServer(this IHostBuilder builder, Action<ServerBuilderContext, ServerBuilder> configure)
    {
        return builder.ConfigureServices((ctx, services) =>
        {
            services.AddHostedService<ServerHostedService>();
            services.AddOptions<ServerHostedServiceOptions>().Configure<IServiceProvider>((opt, sp) =>
            {
                opt.ServerBuilder = new ServerBuilder(sp);
                configure(new ServerBuilderContext(ctx.HostingEnvironment, ctx.Configuration), opt.ServerBuilder);
            });
        });
    }
}