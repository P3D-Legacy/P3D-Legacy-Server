using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Client.P3D.Services;

using System.Linq;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClientP3D(this IServiceCollection services)
        {
            services.AddHostedService<P3DPlayerMovementCompensationService>();

            services.AddScoped<P3DConnectionContextHandler>();
            services.AddNotificationsEnumerable(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<P3DConnectionContextHandler>());

            services.AddScoped<P3DProtocol>();

            return services;
        }
    }
}