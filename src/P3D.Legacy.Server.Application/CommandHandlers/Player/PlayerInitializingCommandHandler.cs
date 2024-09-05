using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.Infrastructure.Repositories.Bans;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player;

internal sealed class PlayerInitializingCommandHandler : ICommandHandler<PlayerInitializingCommand>
{
    private readonly IPlayerOriginGenerator _playerIdGenerator;
    private readonly IPlayerContainerWriterAsync _playerContainer;
    private readonly IBanRepository _banRepository;

    public PlayerInitializingCommandHandler(IPlayerContainerWriterAsync playerContainer, IPlayerOriginGenerator playerIdGenerator, IBanRepository banRepository)
    {
            _playerIdGenerator = playerIdGenerator ?? throw new ArgumentNullException(nameof(playerIdGenerator));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

    public async Task<CommandResult> HandleAsync(PlayerInitializingCommand command, CancellationToken ct)
    {
            var player = command.Player;

            Debug.Assert(player.State == PlayerState.Initializing);

            var gameJoltId = await player.GetGameJoltIdOrNoneAsync(ct);
            var playerId = gameJoltId.IsNone ? PlayerId.FromName(player.Name) : PlayerId.FromGameJolt(gameJoltId);

            if (await _banRepository.GetAsync(playerId, ct) is { } banEntity)
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