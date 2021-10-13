using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommandHandlers
{
    public class PlayerInitializingCommandHandler : IRequestHandler<PlayerInitializingCommand>
    {
        private readonly ILogger _logger;
        private readonly IPlayerIdGenerator _playerIdGenerator;

        public PlayerInitializingCommandHandler(ILogger<PlayerInitializingCommandHandler> logger, IPlayerIdGenerator playerIdGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerIdGenerator = playerIdGenerator ?? throw new ArgumentNullException(nameof(playerIdGenerator));
        }

        public async Task<Unit> Handle(PlayerInitializingCommand request, CancellationToken ct)
        {
            var id = await _playerIdGenerator.GenerateAsync(ct);
            await request.Player.AssignIdAsync(id, ct);

            return Unit.Value;
        }
    }
}