using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.GUI.Services;
using P3D.Legacy.Server.GUI.Views;

namespace P3D.Legacy.Server.GUI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGUI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<UIServiceScopeFactory>();

            services.AddSingleton<UILogEventSink>();

            services.AddSingleton<ThreadGUIManagerService>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ThreadGUIManagerService>(sp => sp.GetRequiredService<ThreadGUIManagerService>()));


            services.AddScoped<ServerUI>();

            services.AddScoped<PlayerTabView>();
            services.AddNotifications(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetService<PlayerTabView>());

            services.AddScoped<ChatTabView>();
            services.AddNotifications(sp => sp.GetRequiredService<UIServiceScopeFactory>().GetService<ChatTabView>());

            services.AddScoped<LogsTabView>();

            services.AddScoped<SettingsTabView>();

            return services;
        }
    }
}