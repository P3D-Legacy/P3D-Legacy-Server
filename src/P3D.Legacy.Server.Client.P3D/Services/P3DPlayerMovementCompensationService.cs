using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.CQERS.Events;

using RateLimiter;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.Services
{
    /// <summary>
    /// Instead of having the player handler sending all movement packets, let an external service
    /// handle that
    /// </summary>
    [SuppressMessage("Performance", "CA1812")]
    internal class P3DPlayerMovementCompensationService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IPlayerContainerReader _playerContainer;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly TimeLimiter _timeLimiter;

        public P3DPlayerMovementCompensationService(ILogger<P3DPlayerMovementCompensationService> logger, TracerProvider traceProvider, IPlayerContainerReader playerContainer, IEventDispatcher eventDispatcher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Client.P3D");
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _timeLimiter = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(1000D / 60D));
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            using var span = _tracer.StartActiveSpan("P3D Player Movement Compensation Service");

            async Task LoopAction()
            {
                foreach (var group in _playerContainer.GetAll().OfType<IP3DPlayerState>().GroupBy(static x => x.LevelFile, StringComparer.Ordinal))
                    foreach (var player in group.Where(static x => x.Moving).OfType<IPlayer>())
                        await _eventDispatcher.DispatchAsync(new PlayerUpdatedPositionEvent(player), ct);
            }

            while (!ct.IsCancellationRequested)
            {
                await _timeLimiter.Enqueue(LoopAction, ct);
            }
        }
    }
}