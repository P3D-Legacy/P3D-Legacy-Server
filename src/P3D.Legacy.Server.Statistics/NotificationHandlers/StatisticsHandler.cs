using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Infrastructure.Services.Statistics;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Statistics.NotificationHandlers
{
    internal sealed class StatisticsHandler :
        INotificationHandler<PlayerUpdatedStateNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<PlayerSentLocalMessageNotification>,
        INotificationHandler<PlayerSentPrivateMessageNotification>,
        INotificationHandler<PlayerSentCommandNotification>,
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>,
        INotificationHandler<WorldUpdatedNotification>
    {
        private readonly ILogger _logger;
        private readonly IStatisticsManager _statisticsManager;

        public StatisticsHandler(ILogger<StatisticsHandler> logger, IStatisticsManager statisticsManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _statisticsManager = statisticsManager ?? throw new ArgumentNullException(nameof(statisticsManager));
        }

        public async Task Handle(PlayerUpdatedStateNotification notification, CancellationToken ct)
        {
            var player = notification.Player;
            if (player is IP3DPlayerState state)
            {
                await _statisticsManager.IncrementActionAsync(player.Id, "player_update_state", ct);
            }
        }

        public async Task Handle(PlayerTriggeredEventNotification notification, CancellationToken ct)
        {
            await _statisticsManager.IncrementActionAsync(notification.Player.Id, $"event_{notification.EventMessage}", ct);
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            await _statisticsManager.IncrementActionAsync(notification.Player.Id, "message_global", ct);
        }

        public async Task Handle(PlayerSentLocalMessageNotification notification, CancellationToken ct)
        {
            await _statisticsManager.IncrementActionAsync(notification.Player.Id, "message_local", ct);
        }

        public async Task Handle(PlayerSentPrivateMessageNotification notification, CancellationToken ct)
        {
            await _statisticsManager.IncrementActionAsync(notification.Player.Id, "message_private", ct);
        }

        public async Task Handle(PlayerSentCommandNotification notification, CancellationToken ct)
        {
            await _statisticsManager.IncrementActionAsync(notification.Player.Id, "message_command", ct);
        }

        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            await _statisticsManager.IncrementActionAsync(notification.Player.Id, "player_joined", ct);
        }

        public async Task Handle(PlayerLeftNotification notification, CancellationToken ct)
        {
            await _statisticsManager.IncrementActionAsync(notification.Id, "player_left", ct);
        }

        public async Task Handle(WorldUpdatedNotification notification, CancellationToken ct)
        {
            if (notification.State.Season != notification.OldState.Season)
            {
                await _statisticsManager.IncrementActionAsync("world_update_season", ct);
            }
            if (notification.State.Weather != notification.OldState.Weather)
            {
                await _statisticsManager.IncrementActionAsync("world_update_weather", ct);
            }
            if (notification.State.Time != notification.OldState.Time)
            {
                await _statisticsManager.IncrementActionAsync("world_update_time", ct);
            }
        }
    }
}