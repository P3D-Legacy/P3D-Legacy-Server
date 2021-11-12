using BetterHostedServices;

using Discord;
using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.DiscordBot.BackgroundServices;

namespace P3D.Legacy.Server.DiscordBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordBotMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            notificationRegistrar.Add(sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerJoinedNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerLeftNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerSentGlobalMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<ServerMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerTriggeredEventNotification>);

            return services;
        }

        public static IServiceCollection AddDiscordBot(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedServiceAsSingleton<DiscordPassthroughService>();
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<IDiscordClient, DiscordSocketClient>(sp => sp.GetRequiredService<DiscordSocketClient>());

            return services;
        }
    }
}