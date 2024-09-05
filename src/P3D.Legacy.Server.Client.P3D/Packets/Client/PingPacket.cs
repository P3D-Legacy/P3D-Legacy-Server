namespace P3D.Legacy.Server.Client.P3D.Packets.Client;

public sealed record PingPacket() : P3DPacket(P3DPacketType.Ping);