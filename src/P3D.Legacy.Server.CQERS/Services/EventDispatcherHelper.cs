using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.CQERS.Events;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Services
{
    internal sealed class EventDispatcherHelper<TEvent> where TEvent : IEvent
    {
        private static readonly Action<ILogger, string, Exception?> ExceptionDuringPublish = LoggerMessage.Define<string>(
            LogLevel.Error, default, "Exception during publish! Strategy: {Strategy}");

        private readonly ILogger _logger;
        private readonly IEnumerable<IEventHandler<TEvent>> _handlers;
        /*private readonly IEnumerable<IEventBehavior<TEvent>> _behaviors;*/
        private readonly Dictionary<DispatchStrategy, Func<IEnumerable<Func<IEvent, CancellationToken, Task>>, IEvent, CancellationToken, Task>> _publishStrategies = new();

        public EventDispatcherHelper(ILogger<EventDispatcherHelper<TEvent>> logger, IEnumerable<IEventHandler<TEvent>> handlers/*, IEnumerable<IEventBehavior<TEvent>> behaviors*/)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
            /*_behaviors = behaviors ?? throw new ArgumentNullException(nameof(behaviors));*/

            _publishStrategies[DispatchStrategy.Async] = AsyncContinueOnExceptionAsync;
            _publishStrategies[DispatchStrategy.ParallelNoWait] = ParallelNoWaitAsync;
            _publishStrategies[DispatchStrategy.ParallelWhenAll] = ParallelWhenAllAsync;
            _publishStrategies[DispatchStrategy.ParallelWhenAny] = ParallelWhenAnyAsync;
            _publishStrategies[DispatchStrategy.SyncContinueOnException] = SyncContinueOnExceptionAsync;
            _publishStrategies[DispatchStrategy.SyncStopOnException] = SyncStopOnExceptionAsync;
        }

        public Task DispatchAsync(TEvent @event, DispatchStrategy strategy, CancellationToken ct)
        {
            if (!_publishStrategies.TryGetValue(strategy, out var publishStrategy))
            {
                throw new ArgumentException($"Unknown strategy: {strategy}");
            }

            return publishStrategy(_handlers.Select(handler => (Func<IEvent, CancellationToken, Task>) ((@event2, ct2) => handler.HandleAsync((TEvent) @event2, ct2))), @event, ct);
        }


        private Task ParallelWhenAllAsync(IEnumerable<Func<IEvent, CancellationToken, Task>> handlers, IEvent @event, CancellationToken ct)
        {
            return Task.WhenAll(handlers.Select(handler => handler(@event, ct)).ToImmutableArray());
        }

        private Task ParallelWhenAnyAsync(IEnumerable<Func<IEvent, CancellationToken, Task>> handlers, IEvent @event, CancellationToken ct)
        {
            return Task.WhenAny(handlers.Select(async handler =>
            {
                try
                {
                    await handler(@event, ct);
                }
                catch (Exception e)
                {
                    ExceptionDuringPublish(_logger, nameof(DispatchStrategy.ParallelWhenAny), e);
                }
            }).ToImmutableArray());
        }

        private Task ParallelNoWaitAsync(IEnumerable<Func<IEvent, CancellationToken, Task>> handlers, IEvent @event, CancellationToken ct)
        {
            foreach (var handler in handlers)
            {
                _ = handler(@event, ct);
            }
            return Task.CompletedTask;
        }

        private async Task AsyncContinueOnExceptionAsync(IEnumerable<Func<IEvent, CancellationToken, Task>> handlers, IEvent @event, CancellationToken ct)
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

        private async Task SyncStopOnExceptionAsync(IEnumerable<Func<IEvent, CancellationToken, Task>> handlers, IEvent @event, CancellationToken ct)
        {
            foreach (var handler in handlers)
            {
                await handler(@event, ct);
            }
        }

        private async Task SyncContinueOnExceptionAsync(IEnumerable<Func<IEvent, CancellationToken, Task>> handlers, IEvent @event, CancellationToken ct)
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
}