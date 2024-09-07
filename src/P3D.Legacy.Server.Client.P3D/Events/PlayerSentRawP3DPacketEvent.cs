using P3D.Legacy.Server.Client.P3D.Packets;
using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Events;

namespace P3D.Legacy.Server.Client.P3D.Events;

public sealed record PlayerSentRawP3DPacketEvent(IPlayer Player, P3DPacket Packet) : IEvent;