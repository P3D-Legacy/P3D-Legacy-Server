using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerInitializingCommandHandler : IRequestHandler<PlayerInitializingCommand>
    {
        private readonly ILogger _logger;
        private readonly IPlayerIdGenerator _playerIdGenerator;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerInitializingCommandHandler(
            ILogger<PlayerInitializingCommandHandler> logger,
            IPlayerContainerWriter playerContainer,
            IPlayerIdGenerator playerIdGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerIdGenerator = playerIdGenerator ?? throw new ArgumentNullException(nameof(playerIdGenerator));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<Unit> Handle(PlayerInitializingCommand request, CancellationToken ct)
        {
            var id = await _playerIdGenerator.GenerateAsync(ct);
            await request.Player.AssignIdAsync(id, ct);

            await _playerContainer.AddAsync(request.Player, ct);

            return Unit.Value;
        }
    }
}