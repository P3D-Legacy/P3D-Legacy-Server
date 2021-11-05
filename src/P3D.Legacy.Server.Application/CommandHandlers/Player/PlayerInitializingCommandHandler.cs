using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerInitializingCommandHandler : IRequestHandler<PlayerInitializingCommand>
    {
        private readonly ILogger _logger;
        private readonly IPlayerOriginGenerator _playerIdGenerator;
        private readonly IPlayerContainerWriter _playerContainer;
        private readonly IBanManager _banManager;

        public PlayerInitializingCommandHandler(
            ILogger<PlayerInitializingCommandHandler> logger,
            IPlayerContainerWriter playerContainer,
            IPlayerOriginGenerator playerIdGenerator,
            IBanManager banManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerIdGenerator = playerIdGenerator ?? throw new ArgumentNullException(nameof(playerIdGenerator));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _banManager = banManager ?? throw new ArgumentNullException(nameof(banManager));
        }

        public async Task<Unit> Handle(PlayerInitializingCommand request, CancellationToken ct)
        {
            var playerId = request.Player.GameJoltId.IsNone ? PlayerId.FromName(request.Player.Name) : PlayerId.FromGameJolt(request.Player.GameJoltId);

            if (await _banManager.GetAsync(playerId, ct) is { } banEntity)
            {
                await request.Player.KickAsync("You are banned!", ct);
            }

            await request.Player.AssignIdAsync(playerId, ct);

            var origin = await _playerIdGenerator.GenerateAsync(ct);
            await request.Player.AssignOriginAsync(origin, ct);

            await _playerContainer.AddAsync(request.Player, ct);

            return Unit.Value;
        }
    }
}