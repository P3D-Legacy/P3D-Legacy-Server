using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.Utils;
using P3D.Legacy.Server.Statistics.NotificationHandlers;

namespace P3D.Legacy.Server.Statistics.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStatistics(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            services.AddTransient<P3DPlayerStateStatisticsHandler>();
            notificationRegistrar.Add(sp => sp.GetRequiredService<P3DPlayerStateStatisticsHandler>() as INotificationHandler<PlayerUpdatedStateNotification>);

            return services;
        }
    }
}