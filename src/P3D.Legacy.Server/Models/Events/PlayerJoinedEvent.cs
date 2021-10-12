namespace P3D.Legacy.Server.Models.Events
{
    public record PlayerJoinedEvent(EventPlayerModel Player) : Event;
}