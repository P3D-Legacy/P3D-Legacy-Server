﻿using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Commands.World;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.World
{
    internal sealed class ChangeWorldTimeCommandHandler : ICommandHandler<ChangeWorldTimeCommand>
    {
        private readonly ILogger _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly WorldService _world;

        public ChangeWorldTimeCommandHandler(ILogger<ChangeWorldTimeCommandHandler> logger, IEventDispatcher eventDispatcher, WorldService world)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public async Task<CommandResult> HandleAsync(ChangeWorldTimeCommand command, CancellationToken ct)
        {
            var time = command.Time;

            var oldState = _world.State;
            _world.CurrentTime = time;
            await _eventDispatcher.DispatchAsync(new WorldUpdatedEvent(_world.State, oldState), ct);
            return new CommandResult(true);
        }
    }
}