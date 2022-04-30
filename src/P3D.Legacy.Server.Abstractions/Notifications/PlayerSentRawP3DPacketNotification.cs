using P3D.Legacy.Common.Packets;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerSentRawP3DPacketNotification(IPlayer Player, P3DPacket Packet) : INotification;
}