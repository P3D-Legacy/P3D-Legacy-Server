using P3D.Legacy.Common;
using P3D.Legacy.Server.Client.P3D.Data.P3DDatas;

namespace P3D.Legacy.Server.Client.P3D.Packets.Trade
{
    public sealed record TradeOfferFromClientPacket() : P3DPacket(P3DPacketType.TradeOffer)
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