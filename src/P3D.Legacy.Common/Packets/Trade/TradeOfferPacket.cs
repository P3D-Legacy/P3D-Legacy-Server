using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Trade
{
    public sealed record TradeOfferPacket() : P3DPacket(P3DPacketType.TradeOffer)
    {
        public int DestinationPlayerId { get => DataItemStorage.GetInt32(0); init => DataItemStorage.SetInt32(0, value); }
        public TradeData TradeData { get => new(DataItemStorage.Get(1)); init => DataItemStorage.Set(1, value.ToP3DString()); }

        public void Deconstruct(out int destinationPlayerId, out TradeData tradeData)
        {
            destinationPlayerId = DestinationPlayerId;
            tradeData = TradeData;
        }
    }
}