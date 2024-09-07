using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Events;

public partial class ReceiveContext<TEvent> : IReceiveContectWithPublisher<TEvent> where TEvent : IEvent
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Exception during publish! Strategy: {Strategy}; Event: {@Event}")]
    private partial void ExceptionDuringPublish(string strategy, IEvent @event, Exception exception);

    private delegate Task EventHandler(IEvent @event, CancellationToken ct);

    public TEvent Message { get; }

    private readonly Dictionary<DispatchStrategy, Func<IEnumerable<EventHandler>, IEvent, CancellationToken, Task>> _publishStrategies = new();

    private readonly ILogger _logger;
    private readonly IEnumerable<IEventHandler<TEvent>> _handlers;

    public ReceiveContext(ILogger<ReceiveContext<TEvent>> logger, IEnumerable<IEventHandler<TEvent>> handlers, TEvent @event)
    {
        Message = @event;
        _logger = logger;
        _handlers = handlers;

        _publishStrategies[DispatchStrategy.Async] = AsyncContinueOnExceptionAsync;
        _publishStrategies[DispatchStrategy.ParallelNoWait] = ParallelNoWaitAsync;
        _publishStrategies[DispatchStrategy.ParallelWhenAll] = ParallelWhenAllAsync;
        _publishStrategies[DispatchStrategy.ParallelWhenAny] = ParallelWhenAnyAsync;
        _publishStrategies[DispatchStrategy.SyncContinueOnException] = SyncContinueOnExceptionAsync;
        _publishStrategies[DispatchStrategy.SyncStopOnException] = SyncStopOnExceptionAsync;
    }

    public Task PublishAsync(IEvent @event, DispatchStrategy strategy, CancellationToken ct = default)
    {
        if (!_publishStrategies.TryGetValue(strategy, out var publishStrategy))
        {
            throw new InvalidOperationException($"Unknown strategy: {strategy}");
        }

        return publishStrategy(_handlers.Select(x => (EventHandler) ((_, ct2) => x.HandleAsync(this, ct2))), @event, ct);
    }


    private async Task ParallelWhenAllAsync(IEnumerable<EventHandler> handlers, IEvent @event, CancellationToken ct)
    {
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), ct);
        var completedTask = await Task.WhenAny(Task.WhenAll(handlers.Select(async handler =>
        {
            try
            {
                await handler(@event, ct);
            }
            catch (Exception e)
            {
                ExceptionDuringPublish(nameof(DispatchStrategy.ParallelWhenAll), @event, e);
            }
        })), timeoutTask);

        if (completedTask == timeoutTask)
        {
            _logger.LogError("Exception during publish! ParallelWhenAll timed out after 5 seconds, Event: {@Event}", @event);
        }
    }

    private async Task ParallelWhenAnyAsync(IEnumerable<EventHandler> handlers, IEvent @event, CancellationToken ct)
    {
        await Task.WhenAny(handlers.Select(async handler =>
        {
            try
            {
                await handler(@event, ct);
            }
            catch (Exception e)
            {
                ExceptionDuringPublish(nameof(DispatchStrategy.ParallelWhenAny), @event, e);
            }
        }));
    }

    private Task ParallelNoWaitAsync(IEnumerable<EventHandler> handlers, IEvent @event, CancellationToken ct)
    {
        foreach (var handler in handlers)
        {
            _ = handler(@event, ct);
        }
        return Task.CompletedTask;
    }

    private async Task AsyncContinueOnExceptionAsync(IEnumerable<EventHandler> handlers, IEvent @event, CancellationToken ct)
    {
        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        foreach (var handler in handlers)
        {
            try
            {
                tasks.Add(handler(@event, ct));
            }
            catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
            {
                exceptions.Add(ex);
            }
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (AggregateException ex)
        {
            exceptions.AddRange(ex.Flatten().InnerExceptions);
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            exceptions.Add(ex);
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }

    private async Task SyncStopOnExceptionAsync(IEnumerable<EventHandler> handlers, IEvent @event, CancellationToken ct)
    {
        foreach (var handler in handlers)
        {
            await handler(@event, ct);
        }
    }

    private async Task SyncContinueOnExceptionAsync(IEnumerable<EventHandler> handlers, IEvent @event, CancellationToken ct)
    {
        var exceptions = new List<Exception>();

        foreach (var handler in handlers)
        {
            try
            {
                await handler(@event, ct);
            }
            catch (AggregateException ex)
            {
                exceptions.AddRange(ex.Flatten().InnerExceptions);
            }
            catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }
}