using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.CommunicationAPI.Controllers;
using P3D.Legacy.Server.CommunicationAPI.Services;

using System.Linq;

namespace P3D.Legacy.Server.CommunicationAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommunicationAPIMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            notificationRegistrar.Add(sp => sp.GetRequiredService<WebSocketSubscribtionManager>().Values.OfType<INotificationHandler<PlayerJoinedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<WebSocketSubscribtionManager>().Values.OfType<INotificationHandler<PlayerLeftNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<WebSocketSubscribtionManager>().Values.OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<WebSocketSubscribtionManager>().Values.OfType<INotificationHandler<ServerMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<WebSocketSubscribtionManager>().Values.OfType<INotificationHandler<PlayerTriggeredEventNotification>>());

            return services;
        }

        public static IServiceCollection AddCommunicationAPI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<CommunicationController>();

            services.AddScoped<WebSocketHandlerFactory>();
            services.AddSingleton<WebSocketSubscribtionManager>();

            return services;
        }
    }
}