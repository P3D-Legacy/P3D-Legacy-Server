using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player;

internal sealed class PlayerFinalizingCommandHandler : ICommandHandler<PlayerFinalizingCommand>
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IPlayerContainerWriterAsync _playerContainer;

    public PlayerFinalizingCommandHandler(IEventDispatcher eventDispatcher, IPlayerContainerWriterAsync playerContainer)
    {
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
    }

    public async Task<CommandResult> HandleAsync(PlayerFinalizingCommand command, CancellationToken ct)
    {
        var player = command.Player;

        Debug.Assert(player.State == PlayerState.Finalizing);

        await _playerContainer.RemoveAsync(player, ct);
        await _eventDispatcher.DispatchAsync(new PlayerLeftEvent(player.Id, player.Origin, player.Name), ct);

        return new CommandResult(true);
    }
}