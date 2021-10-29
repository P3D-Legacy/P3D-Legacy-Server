using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Queries.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerAuthenticateGameJoltCommandHandler : IRequestHandler<PlayerAuthenticateGameJoltCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IPermissionQueries _permissionQueries;
        public PlayerAuthenticateGameJoltCommandHandler(ILogger<PlayerAuthenticateGameJoltCommandHandler> logger, IPermissionQueries permissionQueries)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _permissionQueries = permissionQueries ?? throw new ArgumentNullException(nameof(permissionQueries));
        }

        public async Task<CommandResult> Handle(PlayerAuthenticateGameJoltCommand request, CancellationToken ct)
        {
            if (!request.GameJoltId.IsNone)
            {
                var permissions = await _permissionQueries.GetByGameJoltAsync(request.GameJoltId, ct);
                await request.Player.AssignPermissionsAsync(permissions.Permissions, ct);
                return new CommandResult(true);
            }

            return new CommandResult(false);
        }
    }
}