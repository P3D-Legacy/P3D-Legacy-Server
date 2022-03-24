using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Trade
{
    public sealed partial record TradeOfferPacket() : P3DPacket(P3DPacketType.TradeOffer)
    {
        public Origin DestinationPlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }
        public TradeData TradeData { get => new(DataItemStorage.Get(1)); init => DataItemStorage.Set(1, value.ToP3DString()); }

        public void Deconstruct(out Origin destinationPlayerOrigin, out TradeData tradeData)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
            tradeData = TradeData;
        }
    }
}