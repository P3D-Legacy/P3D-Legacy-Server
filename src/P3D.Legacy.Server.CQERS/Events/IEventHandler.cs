using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Events;

public interface IEventHandler { }

public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IEvent
{
    Task HandleAsync(IReceiveContext<TEvent> context, CancellationToken ct);
}

public class EventHandlerWrapper<TEventHandler, TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
    where TEventHandler : IEventHandler<TEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, TEventHandler?> _factory;

    public EventHandlerWrapper(IServiceProvider serviceProvider, Func<IServiceProvider, TEventHandler?> factory)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public Task HandleAsync(IReceiveContext<TEvent> context, CancellationToken ct)
    {
        if (_factory(_serviceProvider) is { } handler)
            return handler.HandleAsync(context, ct);
        return Task.CompletedTask;
    }
}
public class EventHandlerEnumerableWrapper<TEventHandler, TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
    where TEventHandler : IEventHandler<TEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, IEnumerable<TEventHandler>> _factory;

    public EventHandlerEnumerableWrapper(IServiceProvider serviceProvider, Func<IServiceProvider, IEnumerable<TEventHandler>> factory)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task HandleAsync(IReceiveContext<TEvent> context, CancellationToken ct)
    {
        foreach (var handler in _factory(_serviceProvider))
        {
            await handler.HandleAsync(context, ct);
        }
    }
}