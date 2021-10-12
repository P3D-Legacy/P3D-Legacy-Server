namespace P3D.Legacy.Server.Models.Events
{
    public record PlayerLeavedEvent(EventPlayerModel Player) : Event;
}