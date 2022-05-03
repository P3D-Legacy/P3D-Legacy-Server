using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Events;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

using KVP = System.Collections.Generic.KeyValuePair<string, object?>;

namespace P3D.Legacy.Server.Statistics.EventHandlers
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class MetricsHandler :
        IEventHandler<PlayerUpdatedStateEvent>,
        IEventHandler<PlayerTriggeredEventEvent>,
        IEventHandler<PlayerSentGlobalMessageEvent>,
        IEventHandler<PlayerSentLocalMessageEvent>,
        IEventHandler<PlayerSentPrivateMessageEvent>,
        IEventHandler<PlayerSentCommandEvent>,
        IEventHandler<PlayerJoinedEvent>,
        IEventHandler<PlayerLeftEvent>,
        IEventHandler<WorldUpdatedEvent>,
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

        public Task HandleAsync(PlayerUpdatedStateEvent notification, CancellationToken ct)
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

        public Task HandleAsync(PlayerTriggeredEventEvent notification, CancellationToken ct)
        {
            _eventCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("event", notification.Event.EventType));

            return Task.CompletedTask;
        }

        public Task HandleAsync(PlayerSentGlobalMessageEvent notification, CancellationToken ct)
        {
            _messageCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "global"));

            return Task.CompletedTask;
        }

        public Task HandleAsync(PlayerSentLocalMessageEvent notification, CancellationToken ct)
        {
            _messageCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "local"));

            return Task.CompletedTask;
        }

        public Task HandleAsync(PlayerSentPrivateMessageEvent notification, CancellationToken ct)
        {
            _messageCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "private"));

            return Task.CompletedTask;
        }

        public Task HandleAsync(PlayerSentCommandEvent notification, CancellationToken ct)
        {
            _messageCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "command"));

            return Task.CompletedTask;
        }

        public Task HandleAsync(PlayerJoinedEvent notification, CancellationToken ct)
        {
            _queueCounter.Add(1,
                new KVP("id", notification.Player.Id),
                new KVP("type", "joined"));

            return Task.CompletedTask;
        }

        public Task HandleAsync(PlayerLeftEvent notification, CancellationToken ct)
        {
            _queueCounter.Add(1,
                new KVP("id", notification.Id),
                new KVP("type", "left"));

            return Task.CompletedTask;
        }

        public Task HandleAsync(WorldUpdatedEvent notification, CancellationToken ct)
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