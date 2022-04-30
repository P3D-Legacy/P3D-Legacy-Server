using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Notifications;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using INotification = P3D.Legacy.Server.Abstractions.Notifications.INotification;
using INotificationMediatr = MediatR.INotification;

namespace P3D.Legacy.Server.Abstractions.Services
{
    public sealed class NotificationDispatcher : INotificationDispatcher
    {
        private sealed class NotificationMediator : Mediator
        {
            private readonly Func<IEnumerable<Func<INotificationMediatr, CancellationToken, Task>>, INotificationMediatr, CancellationToken, Task> _publish;

            public NotificationMediator(ServiceFactory serviceFactory, Func<IEnumerable<Func<INotificationMediatr, CancellationToken, Task>>, INotificationMediatr, CancellationToken, Task> publish) : base(serviceFactory)
            {
                _publish = publish;
            }

            protected override async Task PublishCore(IEnumerable<Func<INotificationMediatr, CancellationToken, Task>> allHandlers, INotificationMediatr notification, CancellationToken ct)
            {
                await _publish(allHandlers, notification, ct);
            }
        }

        private static readonly Action<ILogger, string, Exception?> ExceptionDuringPublish = LoggerMessage.Define<string>(
            LogLevel.Error, default, "Exception during publish! Strategy: {Strategy}");

        public DispatchStrategy DefaultStrategy { get; set; } = DispatchStrategy.ParallelNoWait;

        private readonly ILogger _logger;
        private readonly Dictionary<DispatchStrategy, IMediator> _publishStrategies = new();

        public NotificationDispatcher(ILogger<NotificationDispatcher> logger, ServiceFactory serviceFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _publishStrategies[DispatchStrategy.Async] = new NotificationMediator(serviceFactory, AsyncContinueOnExceptionAsync);
            _publishStrategies[DispatchStrategy.ParallelNoWait] = new NotificationMediator(serviceFactory, ParallelNoWaitAsync);
            _publishStrategies[DispatchStrategy.ParallelWhenAll] = new NotificationMediator(serviceFactory, ParallelWhenAllAsync);
            _publishStrategies[DispatchStrategy.ParallelWhenAny] = new NotificationMediator(serviceFactory, ParallelWhenAnyAsync);
            _publishStrategies[DispatchStrategy.SyncContinueOnException] = new NotificationMediator(serviceFactory, SyncContinueOnExceptionAsync);
            _publishStrategies[DispatchStrategy.SyncStopOnException] = new NotificationMediator(serviceFactory, SyncStopOnExceptionAsync);
        }

        public Task DispatchAsync<TNotification>(TNotification notification) where TNotification : INotification
        {
            return DispatchAsync(notification, DefaultStrategy, default);
        }

        public Task DispatchAsync<TNotification>(TNotification notification, DispatchStrategy strategy) where TNotification : INotification
        {
            return DispatchAsync(notification, strategy, default);
        }

        public Task DispatchAsync<TNotification>(TNotification notification, CancellationToken ct) where TNotification : INotification
        {
            return DispatchAsync(notification, DefaultStrategy, ct);
        }

        public Task DispatchAsync<TNotification>(TNotification notification, DispatchStrategy strategy, CancellationToken ct) where TNotification : INotification
        {
            if (!_publishStrategies.TryGetValue(strategy, out var mediator))
            {
                throw new ArgumentException($"Unknown strategy: {strategy}");
            }

            return mediator.Publish(notification, ct);
        }

        private Task ParallelWhenAllAsync(IEnumerable<Func<INotificationMediatr, CancellationToken, Task>> handlers, INotificationMediatr notification, CancellationToken ct)
        {
            return Task.WhenAll(handlers.Select(handler => handler(notification, ct)).ToImmutableArray());
        }

        private Task ParallelWhenAnyAsync(IEnumerable<Func<INotificationMediatr, CancellationToken, Task>> handlers, INotificationMediatr notification, CancellationToken ct)
        {
            return Task.WhenAny(handlers.Select(async handler =>
            {
                try
                {
                    await handler(notification, ct);
                }
                catch (Exception e)
                {
                    ExceptionDuringPublish(_logger, nameof(DispatchStrategy.ParallelWhenAny), e);
                }
            }).ToImmutableArray());
        }

        private Task ParallelNoWaitAsync(IEnumerable<Func<INotificationMediatr, CancellationToken, Task>> handlers, INotificationMediatr notification, CancellationToken ct)
        {
            foreach (var handler in handlers)
            {
                _ = handler(notification, ct);
            }
            return Task.CompletedTask;
        }

        private async Task AsyncContinueOnExceptionAsync(IEnumerable<Func<INotificationMediatr, CancellationToken, Task>> handlers, INotificationMediatr notification, CancellationToken ct)
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

        private async Task SyncStopOnExceptionAsync(IEnumerable<Func<INotificationMediatr, CancellationToken, Task>> handlers, INotificationMediatr notification, CancellationToken ct)
        {
            foreach (var handler in handlers)
            {
                await handler(notification, ct);
            }
        }

        private async Task SyncContinueOnExceptionAsync(IEnumerable<Func<INotificationMediatr, CancellationToken, Task>> handlers, INotificationMediatr notification, CancellationToken ct)
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