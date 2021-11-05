using P3D.Legacy.Common.Packets;

using System.Text;

namespace P3D.Legacy.Server.Client.P3D
{
    public sealed class DefaultP3DPacketBuilder : IP3DPacketBuilder
    {
        public byte[] CreateData(P3DPacket packet)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(packet.Protocol).Append('|').Append((byte) packet.Id).Append('|').Append(packet.Origin);

            if (packet.DataItemStorage.Count == 0)
            {
                stringBuilder.Append("|0|");
                return Encoding.ASCII.GetBytes(stringBuilder.Append("\r\n").ToString());
            }

            stringBuilder.Append('|').Append(packet.DataItemStorage.Count).Append("|0|");

            for (int i = 0, pos = 0; i < packet.DataItemStorage.Count - 1; i++)
            {
                // We skip writing 0, it's obvious. Start with the second item
                pos += packet.DataItemStorage.Get(i).Length;
                stringBuilder.Append(pos).Append('|');
            }

            foreach (var dataItem in packet.DataItemStorage)
                stringBuilder.Append(dataItem);

            return Encoding.ASCII.GetBytes(stringBuilder.Append("\r\n").ToString());
        }
    }
}