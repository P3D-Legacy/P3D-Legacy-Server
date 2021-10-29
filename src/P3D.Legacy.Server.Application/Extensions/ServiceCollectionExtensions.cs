using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.CommandHandlers.Administration;
using P3D.Legacy.Server.Application.CommandHandlers.Player;
using P3D.Legacy.Server.Application.CommandHandlers.World;
using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Commands.World;
using P3D.Legacy.Server.Application.Options;
using P3D.Legacy.Server.Application.Queries.Bans;
using P3D.Legacy.Server.Application.Queries.Permissions;
using P3D.Legacy.Server.Application.Queries.Players;
using P3D.Legacy.Server.Application.Services;

using System.Linq;

namespace P3D.Legacy.Server.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            requestRegistrar.Add(sp => sp.GetRequiredService<ChangePlayerPermissionsCommandHandler>() as IRequestHandler<ChangePlayerPermissionsCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerAuthenticateDefaultCommandHandler>() as IRequestHandler<PlayerAuthenticateDefaultCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerAuthenticateGameJoltCommandHandler>() as IRequestHandler<PlayerAuthenticateGameJoltCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerFinalizingCommandHandler>() as IRequestHandler<PlayerFinalizingCommand>);
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerInitializingCommandHandler>() as IRequestHandler<PlayerInitializingCommand>);
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerReadyCommandHandler>() as IRequestHandler<PlayerReadyCommand>);
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerMutedPlayerCommandHandler>() as IRequestHandler<PlayerMutedPlayerCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerUnmutedPlayerCommandHandler>() as IRequestHandler<PlayerUnmutedPlayerCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<BanPlayerCommandHandler>() as IRequestHandler<BanPlayerCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<UnbanPlayerCommandHandler>() as IRequestHandler<UnbanPlayerCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<KickPlayerCommandHandler>() as IRequestHandler<KickPlayerCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<ChangeWorldSeasonCommandHandler>() as IRequestHandler<ChangeWorldSeasonCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<ChangeWorldTimeCommandHandler>() as IRequestHandler<ChangeWorldTimeCommand, CommandResult>);
            requestRegistrar.Add(sp => sp.GetRequiredService<ChangeWorldWeatherCommandHandler>() as IRequestHandler<ChangeWorldWeatherCommand, CommandResult>);

            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerJoinedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerLeavedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerUpdatedStateNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLocalMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<MessageToPlayerNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentRawP3DPacketNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<ServerMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerTriggeredEventNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentCommandNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLoginNotification>>());

            return services;
        }
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ServerOptions>(configuration.GetSection("Server"));


            services.AddSingleton<DefaultJsonSerializer>();

            services.AddSingleton<P3DPacketFactory>();

            services.AddScoped<ConnectionContextHandlerFactory>();

            services.AddSingleton<IPlayerIdGenerator, DefaultPlayerIdGenerator>();

            services.AddTransient<IBanQueries, BanQueries>();
            services.AddTransient<IPlayerQueries, PlayerQueries>();
            services.AddTransient<IPermissionQueries, PermissionQueries>();

            services.AddSingleton<DefaultPlayerContainer>();
            services.AddTransient<IPlayerContainerWriter>(sp => sp.GetRequiredService<DefaultPlayerContainer>());
            services.AddTransient<IPlayerContainerReader>(sp => sp.GetRequiredService<DefaultPlayerContainer>());

            services.AddSingleton<WorldService>();
            services.AddHostedService<WorldService>(sp => sp.GetRequiredService<WorldService>());


            services.AddTransient<ChangePlayerPermissionsCommandHandler>();
            services.AddTransient<PlayerAuthenticateDefaultCommandHandler>();
            services.AddTransient<PlayerAuthenticateGameJoltCommandHandler>();
            services.AddTransient<PlayerFinalizingCommandHandler>();
            services.AddTransient<PlayerInitializingCommandHandler>();
            services.AddTransient<PlayerReadyCommandHandler>();
            services.AddTransient<PlayerMutedPlayerCommandHandler>();
            services.AddTransient<PlayerUnmutedPlayerCommandHandler>();
            services.AddTransient<BanPlayerCommandHandler>();
            services.AddTransient<UnbanPlayerCommandHandler>();
            services.AddTransient<KickPlayerCommandHandler>();
            services.AddTransient<ChangeWorldSeasonCommandHandler>();
            services.AddTransient<ChangeWorldTimeCommandHandler>();
            services.AddTransient<ChangeWorldWeatherCommandHandler>();

            return services;
        }
    }
}