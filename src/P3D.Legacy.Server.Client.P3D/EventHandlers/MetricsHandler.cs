using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Events.Player;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

using KVP = System.Collections.Generic.KeyValuePair<string, object?>;

namespace P3D.Legacy.Server.Client.P3D.EventHandlers;

internal sealed class MetricsHandler :
    IEventHandler<PlayerUpdatedStateEvent>,
    IDisposable
{
    private readonly ILogger _logger;
    private readonly Meter _meter;
    private readonly Counter<long> _stateCounter;

    public MetricsHandler(ILogger<MetricsHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _meter = new Meter("P3D.Legacy.Server.Client.P3D");
        _stateCounter = _meter.CreateCounter<long>("player_p3d_state", "count", "occurrence of tags sent by player");
    }

    public Task HandleAsync(IReceiveContext<PlayerUpdatedStateEvent> context, CancellationToken ct)
    {
        var player = context.Message.Player;
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

    public void Dispose()
    {
        _meter.Dispose();
    }
}