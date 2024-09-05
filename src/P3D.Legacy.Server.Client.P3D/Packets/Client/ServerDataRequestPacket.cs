namespace P3D.Legacy.Server.Client.P3D.Packets.Client;

public sealed record ServerDataRequestPacket() : P3DPacket(P3DPacketType.ServerDataRequest);