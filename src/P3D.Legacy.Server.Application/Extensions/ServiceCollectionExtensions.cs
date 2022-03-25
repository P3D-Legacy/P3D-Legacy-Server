using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.CommandHandlers.Administration;
using P3D.Legacy.Server.Application.CommandHandlers.Player;
using P3D.Legacy.Server.Application.CommandHandlers.World;
using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Commands.World;
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
            requestRegistrar.AddWithRegistration<ChangePlayerPermissionsCommand, CommandResult, ChangePlayerPermissionsCommandHandler>();
            requestRegistrar.AddWithRegistration<PlayerAuthenticateDefaultCommand, CommandResult, PlayerAuthenticateDefaultCommandHandler>();
            requestRegistrar.AddWithRegistration<PlayerAuthenticateGameJoltCommand, CommandResult, PlayerAuthenticateGameJoltCommandHandler>();
            requestRegistrar.AddWithRegistration<PlayerFinalizingCommand, PlayerFinalizingCommandHandler>();
            requestRegistrar.AddWithRegistration<PlayerInitializingCommand, PlayerInitializingCommandHandler>();
            requestRegistrar.AddWithRegistration<PlayerReadyCommand, PlayerReadyCommandHandler>();
            requestRegistrar.AddWithRegistration<PlayerMutedPlayerCommand, CommandResult, PlayerMutedPlayerCommandHandler>();
            requestRegistrar.AddWithRegistration<PlayerUnmutedPlayerCommand, CommandResult, PlayerUnmutedPlayerCommandHandler>();
            requestRegistrar.AddWithRegistration<BanPlayerCommand, CommandResult, BanPlayerCommandHandler>();
            requestRegistrar.AddWithRegistration<UnbanPlayerCommand, CommandResult, UnbanPlayerCommandHandler>();
            requestRegistrar.AddWithRegistration<KickPlayerCommand, CommandResult, KickPlayerCommandHandler>();
            requestRegistrar.AddWithRegistration<ChangeWorldSeasonCommand, CommandResult, ChangeWorldSeasonCommandHandler>();
            requestRegistrar.AddWithRegistration<ChangeWorldTimeCommand, CommandResult, ChangeWorldTimeCommandHandler>();
            requestRegistrar.AddWithRegistration<ChangeWorldWeatherCommand, CommandResult, ChangeWorldWeatherCommandHandler>();

            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerJoinedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerLeftNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerUpdatedStateNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLocalMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentPrivateMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<MessageToPlayerNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentRawP3DPacketNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<ServerMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerTriggeredEventNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentCommandNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<WorldUpdatedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLoginNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerTradeInitiatedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerTradeAcceptedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerTradeAbortedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerTradeOfferedPokemonNotification>>());

            return services;
        }
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<TradeManager>();

            services.AddSingleton<P3DPacketFactory>();

            services.AddScoped<ConnectionContextHandlerFactory>();

            services.AddSingleton<IPlayerOriginGenerator, DefaultPlayerOriginGenerator>();

            services.AddTransient<IBanQueries, BanQueries>();
            services.AddTransient<IPlayerQueries, PlayerQueries>();
            services.AddTransient<IPermissionQueries, PermissionQueries>();

            services.AddSingleton<DefaultPlayerContainer>();
            services.AddTransient<IPlayerContainerWriter>(sp => sp.GetRequiredService<DefaultPlayerContainer>());
            services.AddTransient<IPlayerContainerReader>(sp => sp.GetRequiredService<DefaultPlayerContainer>());

            services.AddSingleton<WorldService>();
            services.AddHostedService<WorldService>(sp => sp.GetRequiredService<WorldService>());

            return services;
        }
    }
}