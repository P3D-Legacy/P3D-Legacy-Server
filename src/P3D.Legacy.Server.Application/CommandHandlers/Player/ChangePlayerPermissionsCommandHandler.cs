using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Infrastructure.Services.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class ChangePlayerPermissionsCommandHandler : IRequestHandler<ChangePlayerPermissionsCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IPermissionManager _permissionManager;

        public ChangePlayerPermissionsCommandHandler(ILogger<ChangePlayerPermissionsCommandHandler> logger, IMediator mediator, IPermissionManager permissionManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        }

        public async Task<CommandResult> Handle(ChangePlayerPermissionsCommand request, CancellationToken ct)
        {
            var result = await _permissionManager.SetPermissionsAsync(request.Player.Id, request.Permissions, ct);
            if (result)
            {
                await request.Player.AssignPermissionsAsync(request.Permissions, ct);
            }

            return new CommandResult(result);
        }
    }
}