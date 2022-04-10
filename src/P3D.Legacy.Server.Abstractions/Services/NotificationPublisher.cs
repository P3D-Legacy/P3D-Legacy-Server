using MediatR;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ZLogger;

namespace P3D.Legacy.Server.Abstractions.Services
{
    internal class NotificationMediator : Mediator
    {
        private readonly Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> _publish;

        public NotificationMediator(ServiceFactory serviceFactory, Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish) : base(serviceFactory)
        {
            _publish = publish;
        }

        protected override async Task PublishCore(IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers, INotification notification, CancellationToken cancellationToken)
        {
            await _publish(allHandlers, notification, cancellationToken);
        }
    }

    /// <summary>
    /// Strategy to use when publishing notifications
    /// </summary>
    public enum PublishStrategy
    {
        /// <summary>
        /// Run each notification handler after one another. Returns when all handlers are finished. In case of any exception(s), they will be captured in an AggregateException.
        /// </summary>
        SyncContinueOnException = 0,

        /// <summary>
        /// Run each notification handler after one another. Returns when all handlers are finished or an exception has been thrown. In case of an exception, any handlers after that will not be run.
        /// </summary>
        SyncStopOnException = 1,

        /// <summary>
        /// Run all notification handlers asynchronously. Returns when all handlers are finished. In case of any exception(s), they will be captured in an AggregateException.
        /// </summary>
        Async = 2,

        /// <summary>
        /// Run each notification handler on it's own thread using Task.Run(). Returns immediately and does not wait for any handlers to finish. Note that you cannot capture any exceptions, even if you await the call to Publish.
        /// </summary>
        ParallelNoWait = 3,

        /// <summary>
        /// Run each notification handler on it's own thread using Task.Run(). Returns when all threads (handlers) are finished. In case of any exception(s), they are captured in an AggregateException by Task.WhenAll.
        /// </summary>
        ParallelWhenAll = 4,

        /// <summary>
        /// Run each notification handler on it's own thread using Task.Run(). Returns when any thread (handler) is finished. Note that you cannot capture any exceptions (See msdn documentation of Task.WhenAny)
        /// </summary>
        ParallelWhenAny = 5,
    }

    public class NotificationPublisher
    {
        public PublishStrategy DefaultStrategy { get; set; } = PublishStrategy.ParallelNoWait;

        private readonly ILogger _logger;
        private readonly Dictionary<PublishStrategy, IMediator> _publishStrategies = new();

        public NotificationPublisher(ILogger<NotificationPublisher> logger, ServiceFactory serviceFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _publishStrategies[PublishStrategy.Async] = new NotificationMediator(serviceFactory, AsyncContinueOnException);
            _publishStrategies[PublishStrategy.ParallelNoWait] = new NotificationMediator(serviceFactory, ParallelNoWait);
            _publishStrategies[PublishStrategy.ParallelWhenAll] = new NotificationMediator(serviceFactory, ParallelWhenAll);
            _publishStrategies[PublishStrategy.ParallelWhenAny] = new NotificationMediator(serviceFactory, ParallelWhenAny);
            _publishStrategies[PublishStrategy.SyncContinueOnException] = new NotificationMediator(serviceFactory, SyncContinueOnException);
            _publishStrategies[PublishStrategy.SyncStopOnException] = new NotificationMediator(serviceFactory, SyncStopOnException);
        }

        public async Task Publish(INotification notification)
        {
            await Publish(notification, DefaultStrategy, default);
        }

        public async Task Publish(INotification notification, PublishStrategy strategy)
        {
            await Publish(notification, strategy, default);
        }

        public async Task Publish(INotification notification, CancellationToken ct)
        {
            await Publish(notification, DefaultStrategy, ct);
        }

        public async Task Publish(INotification notification, PublishStrategy strategy, CancellationToken ct)
        {
            if (!_publishStrategies.TryGetValue(strategy, out var mediator))
            {
                throw new ArgumentException($"Unknown strategy: {strategy}");
            }

            await mediator.Publish(notification, ct);
        }

        private async Task ParallelWhenAll(IEnumerable<Func<INotification, CancellationToken, Task>> handlers, INotification notification, CancellationToken ct)
        {
            await Task.WhenAll(handlers.Select(handler => Task.Run(() => handler(notification, ct), ct)));
        }

        private async Task ParallelWhenAny(IEnumerable<Func<INotification, CancellationToken, Task>> handlers, INotification notification, CancellationToken ct)
        {
            await Task.WhenAny(handlers.Select(handler => Task.Run(() => handler(notification, ct).ContinueWith(task =>
            {
                if (task.Exception is not null)
                {
                    _logger.ZLogError(task.Exception, "Exception during publish! Strategy: {Strategy}", nameof(PublishStrategy.ParallelWhenAny));
                }
            }, ct), ct)));
        }

        private Task ParallelNoWait(IEnumerable<Func<INotification, CancellationToken, Task>> handlers, INotification notification, CancellationToken ct)
        {
            foreach (var handler in handlers)
            {
                Task.Run(() => handler(notification, ct), ct).ContinueWith(task =>
                {
                    if (task.Exception is not null)
                    {
                        _logger.ZLogError(task.Exception, "Exception during publish! Strategy: {Strategy}", nameof(PublishStrategy.ParallelNoWait));
                    }
                }, ct);
            }

            return Task.CompletedTask;
        }

        private async Task AsyncContinueOnException(IEnumerable<Func<INotification, CancellationToken, Task>> handlers, INotification notification, CancellationToken ct)
        {
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    tasks.Add(handler(notification, ct));
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

        private async Task SyncStopOnException(IEnumerable<Func<INotification, CancellationToken, Task>> handlers, INotification notification, CancellationToken ct)
        {
            foreach (var handler in handlers)
            {
                await handler(notification, ct);
            }
        }

        private async Task SyncContinueOnException(IEnumerable<Func<INotification, CancellationToken, Task>> handlers, INotification notification, CancellationToken ct)
        {
            var exceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    await handler(notification, ct);
                }
                catch (AggregateException ex)
                {
                    exceptions.AddRange(ex.Flatten().InnerExceptions);
                }
                catch (Exception ex) when (!(ex is OutOfMemoryException or StackOverflowException))
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