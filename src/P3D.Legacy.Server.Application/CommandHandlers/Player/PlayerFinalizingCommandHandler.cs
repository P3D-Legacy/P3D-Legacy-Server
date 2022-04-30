using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class PlayerFinalizingCommandHandler : ICommandHandler<PlayerFinalizingCommand>
    {
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerFinalizingCommandHandler(INotificationDispatcher notificationDispatcher, IPlayerContainerWriter playerContainer)
        {
            _notificationDispatcher = notificationDispatcher ?? throw new ArgumentNullException(nameof(notificationDispatcher));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<CommandResult> Handle(PlayerFinalizingCommand request, CancellationToken ct)
        {
            var player = request.Player;

            Debug.Assert(player.State == PlayerState.Finalizing);

            await _playerContainer.RemoveAsync(player, ct);
            await _notificationDispatcher.DispatchAsync(new PlayerLeftNotification(player.Id, player.Origin, player.Name), ct);

            return new CommandResult(true);
        }
    }
}