namespace P3D.Legacy.Server.Models.Events
{
    public record PlayerGlobalMessageEvent(EventPlayerModel Player, string Message) : Event;
}