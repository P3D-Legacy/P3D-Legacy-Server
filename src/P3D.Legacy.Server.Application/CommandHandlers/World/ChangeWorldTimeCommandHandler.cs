using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.World;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.World
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class ChangeWorldTimeCommandHandler : ICommandHandler<ChangeWorldTimeCommand>
    {
        private readonly ILogger _logger;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly WorldService _world;

        public ChangeWorldTimeCommandHandler(ILogger<ChangeWorldTimeCommandHandler> logger, INotificationDispatcher notificationDispatcher, WorldService world)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationDispatcher = notificationDispatcher ?? throw new ArgumentNullException(nameof(notificationDispatcher));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public async Task<CommandResult> Handle(ChangeWorldTimeCommand request, CancellationToken ct)
        {
            var oldState = _world.State;
            _world.CurrentTime = request.Time;
            await _notificationDispatcher.DispatchAsync(new WorldUpdatedNotification(_world.State, oldState), ct);
            return new CommandResult(true);
        }
    }
}