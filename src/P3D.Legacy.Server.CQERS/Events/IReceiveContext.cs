namespace P3D.Legacy.Server.CQERS.Events
{
    public interface IReceiveContext<out TEvent> where TEvent : IEvent
    {
        TEvent Message { get; }
    }
}