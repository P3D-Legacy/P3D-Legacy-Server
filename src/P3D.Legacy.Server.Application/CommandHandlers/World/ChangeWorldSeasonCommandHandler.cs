using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;
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
        private readonly NotificationPublisher _notificationPublisher;
        private readonly WorldService _world;

        public ChangeWorldSeasonCommandHandler(ILogger<ChangeWorldSeasonCommandHandler> logger, NotificationPublisher notificationPublisher, WorldService world)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public async Task<CommandResult> Handle(ChangeWorldSeasonCommand request, CancellationToken ct)
        {
            var oldState = _world.State;
            _world.Season = request.Season;
            await _notificationPublisher.Publish(new WorldUpdatedNotification(_world.State, oldState), ct);
            return new CommandResult(true);
        }
    }
}