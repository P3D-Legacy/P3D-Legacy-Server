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
    internal sealed class P3DPlayerStateStatisticsHandler : INotificationHandler<PlayerUpdatedStateNotification>, IDisposable
    {
        private readonly ILogger _logger;
        private readonly Meter _meter;
        private readonly Counter<long> _counter;

        public P3DPlayerStateStatisticsHandler(ILogger<P3DPlayerStateStatisticsHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _meter = new Meter("P3D.Legacy.Server.Statistics");
            _counter = _meter.CreateCounter<long>("player_p3d_state", "count", "occurrence of tags sent by player");
        }

        public Task Handle(PlayerUpdatedStateNotification notification, CancellationToken cancellationToken)
        {
            var player = notification.Player;
            if (player is IP3DPlayerState state)
            {
                _counter.Add(1, new KVP("location", state.LevelFile));
                _counter.Add(1, new KVP("decimal_separator", state.DecimalSeparator));
                _counter.Add(1, new KVP("gamemode", state.GameMode));
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _meter.Dispose();
        }
    }
}