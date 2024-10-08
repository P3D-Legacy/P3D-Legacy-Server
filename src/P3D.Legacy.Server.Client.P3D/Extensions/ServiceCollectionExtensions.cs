﻿using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Client.P3D.EventHandlers;
using P3D.Legacy.Server.Client.P3D.Packets;
using P3D.Legacy.Server.Client.P3D.Services;
using P3D.Legacy.Server.Domain.Extensions;
using P3D.Legacy.Server.Domain.Services;

using System.Linq;

namespace P3D.Legacy.Server.Client.P3D.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientP3D(this IServiceCollection services)
    {
        services.AddTransient<MetricsHandler>();
        services.AddEventHandler(static sp => sp.GetRequiredService<MetricsHandler>());

        //services.AddTransient<StatisticsHandler>();
        //services.AddEvents(sp => sp.GetRequiredService<StatisticsHandler>());

        services.AddSingleton<P3DMonsterConverter>();

        services.AddSingleton<IP3DPacketFactory, P3DPacketServerFactory>();

        services.AddHostedService<P3DNATHandler>();
        services.AddSingleton<P3DPlayerMovementCompensationService>();
        services.AddHostedService<P3DPlayerMovementCompensationService>(static sp => sp.GetRequiredService<P3DPlayerMovementCompensationService>());

        services.AddScoped<P3DConnectionContextHandler>();
        services.AddEventHandlers(static sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<P3DConnectionContextHandler>());

        services.AddScoped<P3DProtocol>();

        return services;
    }
}