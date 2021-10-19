using MediatR;

using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Application.Notifications;
using P3D.Legacy.Server.Statistics.NotificationHandlers;

using System;
using System.Collections.Generic;

namespace P3D.Legacy.Server.Statistics.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IEnumerable<(Type, (Type, ServiceLifetime))> AddStatisticsNotifications()
        {
            yield return (typeof(INotificationHandler<PlayerUpdatedStateNotification>), (typeof(P3DPlayerStateStatisticsHandler), ServiceLifetime.Singleton));
        }

        public static IServiceCollection AddStatistics(this IServiceCollection services)
        {

            return services;
        }
    }
}