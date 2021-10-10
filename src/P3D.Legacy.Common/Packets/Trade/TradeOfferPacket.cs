using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Common.Packets.Trade
{
    public class TradeOfferPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.TradeOffer;

        public int DestinationPlayerID { get => int.Parse(DataItems[0] == string.Empty ? 0.ToString() : DataItems[0]); set => DataItems[0] = value.ToString(); }
        public TradeData TradeData { get => DataItems[1]; set => DataItems[1] = value; }
    }
}
