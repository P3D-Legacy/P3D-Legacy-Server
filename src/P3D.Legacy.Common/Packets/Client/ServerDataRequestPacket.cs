namespace P3D.Legacy.Common.Packets.Client
{
    public sealed record ServerDataRequestPacket() : P3DPacket(P3DPacketType.ServerDataRequest);
}