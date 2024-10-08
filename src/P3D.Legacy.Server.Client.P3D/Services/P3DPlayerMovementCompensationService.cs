﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Events.Player;
using P3D.Legacy.Server.Domain.Services;

using RateLimiter;

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.Services;

/// <summary>
/// Instead of having the player handler sending all movement packets, let an external service
/// handle that
/// </summary>
internal class P3DPlayerMovementCompensationService : BackgroundService
{
    public readonly AsyncLocal<bool> IsFromService = new();

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
        // We will overwhelm tracing with this
        //using var span = _tracer.StartActiveSpan("P3D Player Movement Compensation Service", SpanKind.Internal);

        while (!ct.IsCancellationRequested)
        {
            await _timeLimiter.Enqueue(async () =>
            {
                IsFromService.Value = true;
                var playersInLevel = _playerContainer.GetAll()
                    .OfType<IP3DPlayerState>()
                    .GroupBy(static x => x.LevelFile, StringComparer.Ordinal)
                    .Where(static x => x.Skip(1).Any());

                foreach (var players in playersInLevel)
                    foreach (var player in players.Where(static x => x.Moving).OfType<IPlayer>())
                        await _eventDispatcher.DispatchAsync(new PlayerUpdatedStateEvent(player), DispatchStrategy.ParallelNoWait, trace: false, ct);
            }, ct);
        }
    }
}