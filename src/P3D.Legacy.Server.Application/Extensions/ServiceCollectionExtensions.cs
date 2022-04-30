using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.Application.CommandHandlers.Administration;
using P3D.Legacy.Server.Application.CommandHandlers.Player;
using P3D.Legacy.Server.Application.CommandHandlers.Trade;
using P3D.Legacy.Server.Application.CommandHandlers.World;
using P3D.Legacy.Server.Application.QueryHandlers.Ban;
using P3D.Legacy.Server.Application.QueryHandlers.Options;
using P3D.Legacy.Server.Application.QueryHandlers.Permission;
using P3D.Legacy.Server.Application.QueryHandlers.Player;
using P3D.Legacy.Server.Application.QueryHandlers.World;
using P3D.Legacy.Server.Application.Services;

namespace P3D.Legacy.Server.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<TradeManager>();


            services.AddCommandHandler<TradeManager>(sp => sp.GetRequiredService<TradeManager>());
            services.AddCommandHandler<TradeManager>(sp => sp.GetRequiredService<TradeManager>());
            services.AddCommandHandler<TradeManager>(sp => sp.GetRequiredService<TradeManager>());
            services.AddCommandHandler<TradeManager>(sp => sp.GetRequiredService<TradeManager>());

            services.AddCommandHandler<ChangePlayerPermissionsCommandHandler>();
            services.AddCommandHandler<PlayerAuthenticateDefaultCommandHandler>();
            services.AddCommandHandler<PlayerAuthenticateGameJoltCommandHandler>();
            services.AddCommandHandler<PlayerFinalizingCommandHandler>();
            services.AddCommandHandler<PlayerInitializingCommandHandler>();
            services.AddCommandHandler<PlayerReadyCommandHandler>();
            services.AddCommandHandler<PlayerMutedPlayerCommandHandler>();
            services.AddCommandHandler<PlayerUnmutedPlayerCommandHandler>();
            services.AddCommandHandler<BanPlayerCommandHandler>();
            services.AddCommandHandler<UnbanPlayerCommandHandler>();
            services.AddCommandHandler<KickPlayerCommandHandler>();
            services.AddCommandHandler<ChangeWorldSeasonCommandHandler>();
            services.AddCommandHandler<ChangeWorldTimeCommandHandler>();
            services.AddCommandHandler<ChangeWorldWeatherCommandHandler>();

            services.AddQueryHandler<BanQueryHandler>();
            services.AddQueryHandler<GetServerOptionsQueryHandler>();
            services.AddQueryHandler<BanQueryHandler>();
            services.AddQueryHandler<PermissionQueryHandler>();
            services.AddQueryHandler<GetPlayerMuteStateQueryHandler>();
            services.AddQueryHandler<PlayerQueryHandler>();
            services.AddQueryHandler<GetWorldStateQueryHandler>();


            services.AddSingleton<IP3DPacketFactory, P3DPacketServerFactory>();

            services.AddScoped<ConnectionContextHandlerFactory>();

            services.AddSingleton<IPlayerOriginGenerator, DefaultPlayerOriginGenerator>();

            services.AddSingleton<DefaultPlayerContainer>();
            services.AddTransient<IPlayerContainerWriter>(sp => sp.GetRequiredService<DefaultPlayerContainer>());
            services.AddTransient<IPlayerContainerReader>(sp => sp.GetRequiredService<DefaultPlayerContainer>());

            services.AddSingleton<WorldService>();
            services.AddHostedService<WorldService>(sp => sp.GetRequiredService<WorldService>());

            return services;
        }
    }
}