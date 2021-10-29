using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Queries.Permissions;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerReadyCommandHandler : IRequestHandler<PlayerReadyCommand>
    {
        private readonly ILogger _logger;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, IPlayerContainerWriter playerContainer, IPermissionQueries permissionQueries)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public Task<Unit> Handle(PlayerReadyCommand request, CancellationToken ct)
        {
            return Task.FromResult(Unit.Value);
        }
    }
}