using P3D.Legacy.Server.Client.P3D.Data.P3DDatas;

namespace P3D.Legacy.Server.Client.P3D.Packets.Trade
{
    public sealed record TradeOfferToClientPacket() : P3DPacket(P3DPacketType.TradeOffer)
    {
        public TradeData TradeData { get => new(DataItemStorage.Get(0)); init => DataItemStorage.Set(0, value.ToP3DString()); }

        public void Deconstruct(out TradeData tradeData)
        {
            tradeData = TradeData;
        }
    }
}