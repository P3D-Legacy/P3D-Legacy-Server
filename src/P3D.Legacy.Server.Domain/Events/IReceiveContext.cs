namespace P3D.Legacy.Server.Domain.Events;

public interface IReceiveContext<out TEvent> where TEvent : IEvent
{
    TEvent Message { get; }
}