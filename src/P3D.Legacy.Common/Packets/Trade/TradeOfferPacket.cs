using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Trade
{
    public sealed partial record TradeOfferPacket() : P3DPacket(P3DPacketType.TradeOffer)
    {
        [P3DPacketDataItem(0, DataItemType.Origin)]
        public Origin DestinationPlayerOrigin { get; set; }
        [P3DPacketDataItem(1, DataItemType.P3DData)]
        public TradeData TradeData { get; set; }

        public void Deconstruct(out Origin destinationPlayerOrigin, out TradeData tradeData)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
            tradeData = TradeData;
        }
    }
}