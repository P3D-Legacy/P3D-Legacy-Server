﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Extensions;
using P3D.Legacy.Server.GUI.Services;
using P3D.Legacy.Server.GUI.Views;

namespace P3D.Legacy.Server.GUI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGUI(this IServiceCollection services)
    {
        services.AddSingleton<UIServiceScopeFactory>();

        services.AddSingleton<ILoggerProvider, UILoggerProvider>();

        services.AddSingleton<TaskGUIManagerService>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TaskGUIManagerService>(static sp => sp.GetRequiredService<TaskGUIManagerService>()));

        services.AddScoped<ServerUI>();

        services.AddScoped<PlayerTabView>();
        services.AddEventHandler(static sp => sp.GetRequiredService<UIServiceScopeFactory>().GetService<PlayerTabView>());

        services.AddScoped<ChatTabView>();
        services.AddEventHandler(static sp => sp.GetRequiredService<UIServiceScopeFactory>().GetService<ChatTabView>());

        services.AddScoped<LogsTabView>();

        services.AddScoped<SettingsTabView>();

        return services;
    }
}