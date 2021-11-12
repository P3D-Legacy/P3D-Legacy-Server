using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Client.P3D.Options;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClientP3DMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            return services;
        }
        public static IServiceCollection AddClientP3D(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<KestrelServerOptions>, P3DServerOptionsSetup>());

            services.AddSingleton<IP3DPacketBuilder, DefaultP3DPacketBuilder>();
            services.AddScoped<P3DConnectionContextHandler>();
            services.AddScoped<P3DProtocol>();

            return services;
        }
    }
}