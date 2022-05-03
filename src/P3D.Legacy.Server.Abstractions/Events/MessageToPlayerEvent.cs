using P3D.Legacy.Server.CQERS.Events;

namespace P3D.Legacy.Server.Abstractions.Events
{
    public record MessageToPlayerEvent(IPlayer From, IPlayer To, string Message) : IEvent;
}