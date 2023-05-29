using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Events
{
    public class NullReceiveContext<TEvent> : IReceiveContext<TEvent> where TEvent : IEvent
    {
        public TEvent Message { get; }

        public NullReceiveContext(TEvent @event)
        {
            Message = @event;
        }

        public Task PublishAsync(IEvent @event, DispatchStrategy strategy, CancellationToken ct = default) => throw new NotSupportedException();
    }
}