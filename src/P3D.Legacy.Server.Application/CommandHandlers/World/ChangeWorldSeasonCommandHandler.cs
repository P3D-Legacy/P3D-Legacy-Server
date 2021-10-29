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
    internal class ChangeWorldSeasonCommandHandler : IRequestHandler<ChangeWorldSeasonCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly WorldService _world;

        public ChangeWorldSeasonCommandHandler(ILogger<ChangeWorldSeasonCommandHandler> logger, IMediator mediator, WorldService world)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public async Task<CommandResult> Handle(ChangeWorldSeasonCommand request, CancellationToken ct)
        {
            _world.Season = request.Season;
            await _mediator.Publish(new WorldUpdatedNotification(), ct);
            return new CommandResult(true);
        }
    }
}