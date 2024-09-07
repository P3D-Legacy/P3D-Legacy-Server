using OpenTelemetry.Trace;

using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.Domain.Events;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Services;

public sealed class EventDispatcher : IEventDispatcher
{
    public DispatchStrategy DefaultStrategy { get; set; } = DispatchStrategy.ParallelWhenAll;

    private readonly ReceiveContextFactory _receiveContextFactory;
    private readonly Tracer _tracer;

    public EventDispatcher(ReceiveContextFactory receiveContextFactory, TracerProvider traceProvider)
    {
        _receiveContextFactory = receiveContextFactory ?? throw new ArgumentNullException(nameof(receiveContextFactory));
        _tracer = traceProvider.GetTracer("P3D.Legacy.Server.CQERS");
    }

    public async Task DispatchAsync<TEvent>(TEvent @event, DispatchStrategy strategy, bool trace, CancellationToken ct) where TEvent : IEvent
    {
        using var span = trace ? _tracer.StartActiveSpan($"{typeof(TEvent).Name} Handle") : null;

        var receiveContext = _receiveContextFactory.Create(@event);
        await receiveContext.PublishAsync(@event, strategy, ct);
    }
}