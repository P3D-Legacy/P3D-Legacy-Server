using P3D.Legacy.Common;
using P3D.Legacy.Server.Client.P3D.Packets;

namespace P3D.Legacy.Server.Client.P3D;

// PROTOCOL|ID|ORIGIN|DATA ITEM COUNT|0|NTH DATA ITEM POSITION|DATA ITEMS CONCATENATED\r\n

// 0.5|98|0|4|0|1|5|25|01000Put Server Name Here\r\n
public sealed record RawPacketData(Protocol Protocol, P3DPacketType Id, Origin Origin, DataItemStorage DataItems);