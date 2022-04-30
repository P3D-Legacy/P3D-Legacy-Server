using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class PlayerInitializingCommandHandler : ICommandHandler<PlayerInitializingCommand>
    {
        private readonly IPlayerOriginGenerator _playerIdGenerator;
        private readonly IPlayerContainerWriter _playerContainer;
        private readonly IBanManager _banManager;

        public PlayerInitializingCommandHandler(
            IPlayerContainerWriter playerContainer,
            IPlayerOriginGenerator playerIdGenerator,
            IBanManager banManager)
        {
            _playerIdGenerator = playerIdGenerator ?? throw new ArgumentNullException(nameof(playerIdGenerator));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _banManager = banManager ?? throw new ArgumentNullException(nameof(banManager));
        }

        public async Task<CommandResult> Handle(PlayerInitializingCommand request, CancellationToken ct)
        {
            var player = request.Player;

            Debug.Assert(player.State == PlayerState.Initializing);

            var playerId = player.GameJoltId.IsNone ? PlayerId.FromName(player.Name) : PlayerId.FromGameJolt(player.GameJoltId);

            if (await _banManager.GetAsync(playerId, ct) is { } banEntity)
            {
                await player.KickAsync($"You are banned: {banEntity.Reason}", ct);
                return CommandResult.Success;
            }

            await player.AssignIdAsync(playerId, ct);

            var origin = await _playerIdGenerator.GenerateAsync(ct);
            await player.AssignOriginAsync(origin, ct);

            await _playerContainer.AddAsync(player, ct);

            return CommandResult.Success;
        }
    }
}