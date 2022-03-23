using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Client.P3D.Extensions;

using System.Buffers;
using System.Text;

namespace P3D.Legacy.Server.Client.P3D
{
    public sealed class DefaultP3DPacketWriter : IP3DPacketWriter
    {
        private static readonly byte[] Separator = Encoding.ASCII.GetBytes("|");
        private static readonly byte[] Separator2 = Encoding.ASCII.GetBytes("|0|");
        private static readonly byte[] Newline = Encoding.ASCII.GetBytes("\r\n");

        public void WriteData(P3DPacket packet, IBufferWriter<byte> writer)
        {
            var encoder = Encoding.ASCII.GetEncoder();

            writer.Write(packet.Protocol.ToString(), encoder);
            writer.Write(Separator);
            writer.Write((byte) packet.Id, encoder);
            writer.Write(Separator);
            writer.Write(packet.Origin.ToString(), encoder);

            if (packet.DataItemStorage.Count == 0)
            {
                writer.Write(Separator2);
                writer.Write(Newline);
                return;
            }

            writer.Write(Separator);
            writer.Write(packet.DataItemStorage.Count, encoder);
            writer.Write(Separator2);

            for (int i = 0, pos = 0; i < packet.DataItemStorage.Count - 1; i++)
            {
                // We skip writing 0, it's obvious. Start with the second item
                pos += packet.DataItemStorage.Get(i).Length;
                writer.Write(pos, encoder);
                writer.Write(Separator);
            }

            foreach (var dataItem in packet.DataItemStorage)
            {
                writer.Write(dataItem, encoder);
            }

            writer.Write(Newline);
            return;
        }
    }
}