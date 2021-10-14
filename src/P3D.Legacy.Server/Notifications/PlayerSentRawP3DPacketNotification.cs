using MediatR;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Models;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerSentRawP3DPacketNotification(IPlayer Player, P3DPacket Packet) : INotification;
}