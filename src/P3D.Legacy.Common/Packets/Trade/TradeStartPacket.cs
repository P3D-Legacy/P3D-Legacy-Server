namespace P3D.Legacy.Common.Packets.Trade
{
    public sealed partial record TradeStartPacket() : P3DPacket(P3DPacketType.TradeStart)
    {
        [P3DPacketDataItem(0, DataItemType.Origin)]
        public Origin DestinationPlayerOrigin { get; set; }

        public void Deconstruct(out Origin destinationPlayerOrigin)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
        }
    }
}