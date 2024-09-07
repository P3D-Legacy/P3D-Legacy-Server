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

internal sealed class ChangeWorldWeatherCommandHandler : ICommandHandler<ChangeWorldWeatherCommand>
{
    private readonly ILogger _logger;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly WorldService _world;

    public ChangeWorldWeatherCommandHandler(ILogger<ChangeWorldWeatherCommandHandler> logger, IEventDispatcher eventDispatcher, WorldService world)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public async Task<CommandResult> HandleAsync(ChangeWorldWeatherCommand command, CancellationToken ct)
    {
        var weather = command.Weather;

        var oldState = _world.State;
        _world.Weather = weather;
        await _eventDispatcher.DispatchAsync(new WorldUpdatedEvent(_world.State, oldState), ct);
        return new CommandResult(true);
    }
}