using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.Abstractions.Services;
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

namespace P3D.Legacy.Server.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddRequest<ChangePlayerPermissionsCommand, CommandResult, ChangePlayerPermissionsCommandHandler>();
            services.AddRequest<PlayerAuthenticateDefaultCommand, CommandResult, PlayerAuthenticateDefaultCommandHandler>();
            services.AddRequest<PlayerAuthenticateGameJoltCommand, CommandResult, PlayerAuthenticateGameJoltCommandHandler>();
            services.AddRequest<PlayerFinalizingCommand, PlayerFinalizingCommandHandler>();
            services.AddRequest<PlayerInitializingCommand, PlayerInitializingCommandHandler>();
            services.AddRequest<PlayerReadyCommand, PlayerReadyCommandHandler>();
            services.AddRequest<PlayerMutedPlayerCommand, CommandResult, PlayerMutedPlayerCommandHandler>();
            services.AddRequest<PlayerUnmutedPlayerCommand, CommandResult, PlayerUnmutedPlayerCommandHandler>();
            services.AddRequest<BanPlayerCommand, CommandResult, BanPlayerCommandHandler>();
            services.AddRequest<UnbanPlayerCommand, CommandResult, UnbanPlayerCommandHandler>();
            services.AddRequest<KickPlayerCommand, CommandResult, KickPlayerCommandHandler>();
            services.AddRequest<ChangeWorldSeasonCommand, CommandResult, ChangeWorldSeasonCommandHandler>();
            services.AddRequest<ChangeWorldTimeCommand, CommandResult, ChangeWorldTimeCommandHandler>();
            services.AddRequest<ChangeWorldWeatherCommand, CommandResult, ChangeWorldWeatherCommandHandler>();


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