using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Models.Events
{
    public record PlayerGameDataEvent(EventPlayerModel Player, DataItemStorage DataItemStorage) : Event;
}