namespace P3D.Legacy.Common.Packets.Client
{
    public sealed partial record PingPacket() : P3DPacket(P3DPacketType.Ping);
}