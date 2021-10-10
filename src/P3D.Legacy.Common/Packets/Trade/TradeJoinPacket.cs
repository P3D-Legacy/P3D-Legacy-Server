namespace P3D.Legacy.Common.Packets.Trade
{
    public class TradeJoinPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.TradeJoin;

        public int DestinationPlayerID { get => int.Parse(DataItems[0] == string.Empty ? 0.ToString() : DataItems[0]); set => DataItems[0] = value.ToString(); }
    }
}
