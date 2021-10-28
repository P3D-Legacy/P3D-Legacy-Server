using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.World;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.World
{
    public class ChangeWorldTimeCommandHandler : IRequestHandler<ChangeWorldTimeCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly WorldService _world;

        public ChangeWorldTimeCommandHandler(ILogger<ChangeWorldTimeCommandHandler> logger, IMediator mediator, WorldService world)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public async Task<CommandResult> Handle(ChangeWorldTimeCommand request, CancellationToken ct)
        {
            _world.CurrentTime = request.Time;
            await _mediator.Publish(new WorldUpdatedNotification(), ct);
            return new CommandResult(true);
        }
    }
}