using P3D.Legacy.Common.Packets;

using System.Text;

namespace P3D.Legacy.Common
{
    // PROTOCOL|ID|ORIGIN|DATA ITEM COUNT|0|NTH DATA ITEM POSITION|DATA ITEMS CONCATENATED\r\n

    // 0.5|98|0|4|0|1|5|25|01000Put Server Name Here\r\n
    public record RawPacketData(Protocol Protocol, P3DPacketType Id, Origin Origin, DataItemStorage DataItems)
    {
        public string Build()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(Protocol).Append('|').Append((byte) Id).Append('|').Append(Origin);

            if (DataItems.Count == 0)
            {
                stringBuilder.Append("|0|");
                return stringBuilder.ToString();
            }

            stringBuilder.Append('|').Append(DataItems.Count).Append("|0|");

            for (int i = 0, pos = 0; i < DataItems.Count - 1; i++)
            {
                // We skip writing 0, it's obvious. Start with the second item
                pos += DataItems.Get(i).Length;
                stringBuilder.Append(pos).Append('|');
            }

            foreach (var dataItem in DataItems)
                stringBuilder.Append(dataItem);

            return stringBuilder.ToString();
        }
    }
}