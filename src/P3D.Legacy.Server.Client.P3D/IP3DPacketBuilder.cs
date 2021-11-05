using P3D.Legacy.Common.Packets;

namespace P3D.Legacy.Server.Client.P3D
{
    public interface IP3DPacketBuilder
    {
        byte[] CreateData(P3DPacket packet);
    }
}