using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.CQERS.Extensions;
using P3D.Legacy.Server.Statistics.EventHandlers;

namespace P3D.Legacy.Server.Statistics.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStatistics(this IServiceCollection services)
    {
        services.AddTransient<MetricsHandler>();
        services.AddEventHandler(static sp => sp.GetRequiredService<MetricsHandler>());

        //services.AddTransient<StatisticsHandler>();
        //services.AddEvents(sp => sp.GetRequiredService<StatisticsHandler>());

        return services;
    }
}