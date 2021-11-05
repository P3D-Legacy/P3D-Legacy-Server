using P3D.Legacy.Common.Packets;

namespace P3D.Legacy.Common
{
    // PROTOCOL|ID|ORIGIN|DATA ITEM COUNT|0|NTH DATA ITEM POSITION|DATA ITEMS CONCATENATED\r\n

    // 0.5|98|0|4|0|1|5|25|01000Put Server Name Here\r\n
    public record RawPacketData(Protocol Protocol, P3DPacketType Id, Origin Origin, DataItemStorage DataItems);
}