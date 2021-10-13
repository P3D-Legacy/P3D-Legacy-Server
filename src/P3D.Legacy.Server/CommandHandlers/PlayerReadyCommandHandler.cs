﻿using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Models;
using P3D.Legacy.Server.Queries.Permissions;
using P3D.Legacy.Server.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommandHandlers
{
    public class PlayerReadyCommandHandler : IRequestHandler<PlayerReadyCommand>
    {
        private readonly ILogger _logger;
        private readonly IPlayerContainerWriter _playerContainer;
        private readonly IPermissionQueries _permissionQueries;

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, IPlayerContainerWriter playerContainer, IPermissionQueries permissionQueries)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _permissionQueries = permissionQueries ?? throw new ArgumentNullException(nameof(permissionQueries));
        }

        public async Task<Unit> Handle(PlayerReadyCommand request, CancellationToken ct)
        {
            var permissions = await _permissionQueries.GetByGameJoltAsync(request.Player.GameJoltId, ct) ?? new PermissionViewModel(Permissions.UnVerified);
            await request.Player.AssignPermissionsAsync(permissions.Permissions, ct);

            await _playerContainer.AddAsync(request.Player, ct);

            return Unit.Value;
        }
    }
}