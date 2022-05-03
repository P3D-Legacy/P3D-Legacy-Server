using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.CQERS.Events;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Services
{
    public sealed class EventDispatcher : IEventDispatcher
    {
        public DispatchStrategy DefaultStrategy { get; set; } = DispatchStrategy.ParallelNoWait;

        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task DispatchAsync<TEvent>(TEvent @event, DispatchStrategy strategy, CancellationToken ct) where TEvent : IEvent
        {
            return _serviceProvider.GetRequiredService<EventDispatcherHelper<TEvent>>().DispatchAsync(@event, strategy, ct);
        }
    }
}