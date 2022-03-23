using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;

using System;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

using KVP = System.Collections.Generic.KeyValuePair<string, object?>;

namespace P3D.Legacy.Server.Statistics.NotificationHandlers
{
    internal sealed class MetricsHandler :
        INotificationHandler<PlayerUpdatedStateNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<PlayerSentLocalMessageNotification>,
        INotificationHandler<PlayerSentPrivateMessageNotification>,
        INotificationHandler<PlayerSentCommandNotification>,
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>,
        INotificationHandler<WorldUpdatedNotification>,
        IDisposable
    {
        private readonly ILogger _logger;
        private readonly Meter _meter;
        private readonly Counter<long> _stateCounter;
        private readonly Counter<long> _eventCounter;
        private readonly Counter<long> _messageCounter;
        private readonly Counter<long> _queueCounter;
        private readonly Counter<long> _worldCounter;

        public MetricsHandler(ILogger<MetricsHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _meter = new Meter("P3D.Legacy.Server.Statistics");
            _stateCounter = _meter.CreateCounter<long>("player_p3d_state", "count", "occurrence of tags sent by player");
            _eventCounter = _meter.CreateCounter<long>("player_event", "count", "occurrence of event triggered by player");
            _messageCounter = _meter.CreateCounter<long>("player_message", "count", "occurrence of messages sent by player");
            _queueCounter = _meter.CreateCounter<long>("player_queue", "count", "occurrence of queue actions by player");
            _worldCounter = _meter.CreateCounter<long>("world_state", "count", "occurrence of world state updates");
        }

        public Task Handle(PlayerUpdatedStateNotification notification, CancellationToken ct)
        {
            var player = notification.Player;
            if (player is IP3DPlayerState state)
            {
                _stateCounter.Add(1,
                    new KVP("id", player.Id),
                    new KVP("location", state.LevelFile),
                    new KVP("decimal_separator", state.DecimalSeparator),
                    new KVP("gamemode", state.GameMode));
            }
            return Task.CompletedTask;
        }

        public Task Handle(PlayerTriggeredEventNotification notification, CancellationToken ct)
        {
            _eventCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("event", notification.EventMessage));

            return Task.CompletedTask;
        }

        public Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            _messageCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "global"));

            return Task.CompletedTask;
        }

        public Task Handle(PlayerSentLocalMessageNotification notification, CancellationToken ct)
        {
            _messageCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "local"));

            return Task.CompletedTask;
        }

        public Task Handle(PlayerSentPrivateMessageNotification notification, CancellationToken ct)
        {
            _messageCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "private"));

            return Task.CompletedTask;
        }

        public Task Handle(PlayerSentCommandNotification notification, CancellationToken ct)
        {
            _messageCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "command"));

            return Task.CompletedTask;
        }

        public Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            _queueCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "joined"));

            return Task.CompletedTask;
        }

        public Task Handle(PlayerLeftNotification notification, CancellationToken ct)
        {
            _queueCounter.Add(1,
                new KVP("id", notification.Id),
                new KVP("type", "left"));

            return Task.CompletedTask;
        }

        public Task Handle(WorldUpdatedNotification notification, CancellationToken ct)
        {
            _worldCounter.Add(1,
                new KVP("season", notification.State.Season),
                new KVP("weather", notification.State.Weather));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _meter.Dispose();
        }
    }
}