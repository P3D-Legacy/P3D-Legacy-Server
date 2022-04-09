using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.GUI.Services;
using P3D.Legacy.Server.GUI.Views;

namespace P3D.Legacy.Server.GUI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGUIMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            notificationRegistrar.Add(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetRequiredService<ChatTabView>() as INotificationHandler<PlayerJoinedNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetRequiredService<ChatTabView>() as INotificationHandler<PlayerLeftNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetRequiredService<ChatTabView>() as INotificationHandler<PlayerSentGlobalMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetRequiredService<ChatTabView>() as INotificationHandler<ServerMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetRequiredService<ChatTabView>() as INotificationHandler<PlayerTriggeredEventNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetRequiredService<ChatTabView>() as INotificationHandler<MessageToPlayerNotification>);

            notificationRegistrar.Add(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetRequiredService<PlayerTabView>() as INotificationHandler<PlayerJoinedNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetRequiredService<PlayerTabView>() as INotificationHandler<PlayerLeftNotification>);

            return services;
        }

        public static IServiceCollection AddGUI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<UIServiceScopeFactory>();

            services.AddSingleton<UILogEventSink>();

            //services.AddSingleton<ThreadGUIManagerService>();
            //services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ThreadGUIManagerService>(sp => sp.GetRequiredService<ThreadGUIManagerService>()));
            services.AddSingleton<TaskGUIManagerService>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TaskGUIManagerService>(sp => sp.GetRequiredService<TaskGUIManagerService>()));

            services.AddScoped<ServerUI>();
            services.AddScoped<PlayerTabView>();
            services.AddScoped<ChatTabView>();
            services.AddScoped<LogsTabView>();
            services.AddScoped<SettingsTabView>();

            return services;
        }
    }
}