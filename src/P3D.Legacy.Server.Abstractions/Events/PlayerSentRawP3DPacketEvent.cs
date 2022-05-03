using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.CQERS.Events;

namespace P3D.Legacy.Server.Abstractions.Events
{
    public sealed record PlayerSentRawP3DPacketEvent(IPlayer Player, P3DPacket Packet) : IEvent;
}