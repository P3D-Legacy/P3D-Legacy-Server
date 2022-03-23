using P3D.Legacy.Common.Packets;

using System.Buffers;

namespace P3D.Legacy.Server.Client.P3D
{
    public interface IP3DPacketWriter
    {
        void WriteData(P3DPacket packet, IBufferWriter<byte> writer);
    }
}