using Bedrock.Framework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;

namespace P3D.Legacy.Server.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureServer(this IHostBuilder builder, Action<ServerBuilderContext, ServerBuilder> configure)
        {
            return builder.ConfigureServices((ctx, services) =>
            {
                services.AddHostedService<ServerHostedService>();
                services.AddOptions<ServerHostedServiceOptions>().Configure<IServiceProvider>((options, sp) =>
                {
                    options.ServerBuilder = new ServerBuilder(sp);
                    configure(new ServerBuilderContext(ctx.HostingEnvironment, ctx.Configuration), options.ServerBuilder);
                });
            });
        }
    }
}