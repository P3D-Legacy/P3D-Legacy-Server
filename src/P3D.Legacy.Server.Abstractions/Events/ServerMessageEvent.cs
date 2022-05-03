using P3D.Legacy.Server.CQERS.Events;

namespace P3D.Legacy.Server.Abstractions.Events
{
    public record ServerMessageEvent(string Message) : IEvent;
}