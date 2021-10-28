using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.CommunicationAPI.Services;

using System.Linq;

namespace P3D.Legacy.Server.CommunicationAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordAPI(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            services.AddTransient<DiscordController>();

            services.AddSingleton<SubscriberManager>();
            notificationRegistrar.Add(sp => sp.GetRequiredService<SubscriberManager>().GetActive().OfType<INotificationHandler<PlayerJoinedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<SubscriberManager>().GetActive().OfType<INotificationHandler<PlayerLeavedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<SubscriberManager>().GetActive().OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<SubscriberManager>().GetActive().OfType<INotificationHandler<ServerMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<SubscriberManager>().GetActive().OfType<INotificationHandler<PlayerTriggeredEventNotification>>());

            return services;
        }
    }
}