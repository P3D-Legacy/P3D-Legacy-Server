using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Infrastructure.Services.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerReadyCommandHandler : IRequestHandler<PlayerReadyCommand>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IPermissionManager _permissionManager;

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, IMediator mediator, IPermissionManager permissionManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        }

        public async Task<Unit> Handle(PlayerReadyCommand request, CancellationToken ct)
        {
            var permissions = await _permissionManager.GetByIdAsync(request.Player.Id, ct);
            await request.Player.AssignPermissionsAsync(permissions.Permissions, ct);

            await _mediator.Publish(new PlayerJoinedNotification(request.Player), ct);
            return Unit.Value;
        }
    }
}