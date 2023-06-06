using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Client.P3D.Packets.Trade
{
    public sealed record TradeStartPacket() : P3DPacket(P3DPacketType.TradeStart)
    {
        public Origin DestinationPlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin destinationPlayerOrigin)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
        }
    }
}