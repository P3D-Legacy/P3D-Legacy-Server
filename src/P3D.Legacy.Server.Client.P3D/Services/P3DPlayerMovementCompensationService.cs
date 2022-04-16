using ComposableAsync;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Application.Services;

using RateLimiter;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.Services
{
    /// <summary>
    /// Instead of having the player handler sending all movement packets, let an external service
    /// handle that
    /// </summary>
    internal class P3DPlayerMovementCompensationService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IPlayerContainerReader _playerContainer;
        private readonly NotificationPublisher _notificationPublisher;
        private readonly TimeLimiter _timeLimiter;

        public P3DPlayerMovementCompensationService(ILogger<P3DPlayerMovementCompensationService> logger, IPlayerContainerReader playerContainer, NotificationPublisher notificationPublisher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
            _timeLimiter = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(1000D / 60D));
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await _timeLimiter;

                await foreach (var group in _playerContainer.GetAllAsync(ct).OfType<IP3DPlayerState>().GroupBy(x => x.LevelFile).WithCancellation(ct))
                {
                    var players = await group.Where(x => x.Moving).OfType<IPlayer>().ToArrayAsync(ct);
                    foreach (var player in players)
                        await _notificationPublisher.Publish(new PlayerUpdatedPositionNotification(player), ct);
                }
            }
        }
    }
}
