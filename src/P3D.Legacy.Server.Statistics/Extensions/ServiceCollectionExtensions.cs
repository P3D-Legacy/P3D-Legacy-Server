using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.Statistics.NotificationHandlers;

namespace P3D.Legacy.Server.Statistics.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStatistics(this IServiceCollection services)
        {
            services.AddTransient<MetricsHandler>();
            services.AddNotifications(sp => sp.GetRequiredService<MetricsHandler>());

            services.AddTransient<StatisticsHandler>();
            services.AddNotifications(sp => sp.GetRequiredService<StatisticsHandler>());

            return services;
        }
    }
}