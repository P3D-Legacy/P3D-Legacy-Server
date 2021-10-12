﻿using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Notifications;
using P3D.Legacy.Server.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommandHandlers
{
    public class PlayerFinalizingCommandHandler : IRequestHandler<PlayerFinalizingCommand>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerFinalizingCommandHandler(ILogger<PlayerFinalizingCommandHandler> logger, IMediator mediator, IPlayerContainerWriter playerContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<Unit> Handle(PlayerFinalizingCommand request, CancellationToken ct)
        {
            await _playerContainer.RemoveAsync(request.Player, ct);

            await _mediator.Publish(new PlayerLeavedNotification(request.Player.Id, request.Player.Name, request.Player.GameJoltId), ct);

            return Unit.Value;
        }
    }
}
