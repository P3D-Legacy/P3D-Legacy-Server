using P3D.Legacy.Server.Domain.Events;

namespace P3D.Legacy.Server.CQERS.Events;

public class NullReceiveContext<TEvent> : IReceiveContext<TEvent> where TEvent : IEvent
{
    public TEvent Message { get; }

    public NullReceiveContext(TEvent @event)
    {
        Message = @event;
    }
}