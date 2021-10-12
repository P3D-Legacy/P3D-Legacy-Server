using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Commands;
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

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, IPlayerContainerWriter playerContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<Unit> Handle(PlayerReadyCommand request, CancellationToken ct)
        {
            await _playerContainer.AddAsync(request.Player, ct);

            return Unit.Value;
        }
    }
}