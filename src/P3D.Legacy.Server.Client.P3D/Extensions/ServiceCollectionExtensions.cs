using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Client.P3D.EventHandlers;
using P3D.Legacy.Server.Client.P3D.Services;
using P3D.Legacy.Server.CQERS.Extensions;

using System.Linq;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClientP3D(this IServiceCollection services)
        {
            services.AddTransient<MetricsHandler>();
            services.AddEvent(static sp => sp.GetRequiredService<MetricsHandler>());

            //services.AddTransient<StatisticsHandler>();
            //services.AddEvents(sp => sp.GetRequiredService<StatisticsHandler>());

            services.AddHostedService<P3DPlayerMovementCompensationService>();

            services.AddScoped<P3DConnectionContextHandler>();
            services.AddEvents(static sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<P3DConnectionContextHandler>());

            services.AddScoped<P3DProtocol>();

            return services;
        }
    }
}