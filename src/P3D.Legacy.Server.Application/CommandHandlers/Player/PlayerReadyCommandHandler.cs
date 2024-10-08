﻿using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Commands.Player;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Events.Player;
using P3D.Legacy.Server.Domain.Extensions;
using P3D.Legacy.Server.Domain.Repositories;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player;

internal sealed class PlayerReadyCommandHandler : ICommandHandler<PlayerReadyCommand>
{
    private readonly ILogger _logger;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IPermissionRepository _permissionRepository;

    public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, IEventDispatcher eventDispatcher, IPermissionRepository permissionRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public async Task<CommandResult> HandleAsync(PlayerReadyCommand command, CancellationToken ct)
    {
        var player = command.Player;

        Debug.Assert(player.State == PlayerState.Authentication);

        var permissions = await _permissionRepository.GetByIdAsync(player.Id, ct);
        await player.AssignPermissionsAsync(permissions.Permissions, ct);

        await _eventDispatcher.DispatchAsync(new PlayerJoinedEvent(player), ct);
        return CommandResult.Success;
    }
}