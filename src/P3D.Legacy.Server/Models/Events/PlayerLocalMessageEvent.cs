namespace P3D.Legacy.Server.Models.Events
{
    public record PlayerLocalMessageEvent(EventPlayerModel Player, string Location, string Message) : Event;
}