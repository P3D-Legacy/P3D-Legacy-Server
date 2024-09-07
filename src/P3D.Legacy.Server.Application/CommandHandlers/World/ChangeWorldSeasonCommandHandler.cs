using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Commands.World;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Events.World;
using P3D.Legacy.Server.Domain.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.World;

internal sealed class ChangeWorldSeasonCommandHandler : ICommandHandler<ChangeWorldSeasonCommand>
{
    private readonly ILogger _logger;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly WorldService _world;

    public ChangeWorldSeasonCommandHandler(ILogger<ChangeWorldSeasonCommandHandler> logger, IEventDispatcher eventDispatcher, WorldService world)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public async Task<CommandResult> HandleAsync(ChangeWorldSeasonCommand command, CancellationToken ct)
    {
        var season = command.Season;

        var oldState = _world.State;
        _world.Season = season;
        await _eventDispatcher.DispatchAsync(new WorldUpdatedEvent(_world.State, oldState), ct);
        return new CommandResult(true);
    }
}