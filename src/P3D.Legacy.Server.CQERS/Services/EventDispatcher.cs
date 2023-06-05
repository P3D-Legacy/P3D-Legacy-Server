using P3D.Legacy.Server.CQERS.Events;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Services
{
    public sealed class EventDispatcher : IEventDispatcher
    {
        public DispatchStrategy DefaultStrategy { get; set; } = DispatchStrategy.ParallelWhenAll;

        private readonly ReceiveContextFactory _receiveContextFactory;

        public EventDispatcher(ReceiveContextFactory receiveContextFactory)
        {
            _receiveContextFactory = receiveContextFactory ?? throw new ArgumentNullException(nameof(receiveContextFactory));
        }

        public Task DispatchAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IEvent => DispatchAsync(@event, DefaultStrategy, ct);
        public async Task DispatchAsync<TEvent>(TEvent @event, DispatchStrategy strategy, CancellationToken ct) where TEvent : IEvent
        {
            var receiveContext = _receiveContextFactory.Create(@event);
            await receiveContext.PublishAsync(@event, strategy, ct);
        }
    }
}