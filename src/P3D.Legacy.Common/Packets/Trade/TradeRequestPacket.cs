namespace P3D.Legacy.Common.Packets.Trade
{
    public class TradeRequestPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.TradeRequest;

        public int DestinationPlayerID { get => int.Parse(DataItems[0] == string.Empty ? 0.ToString() : DataItems[0]); set => DataItems[0] = value.ToString(); }
    }
}
