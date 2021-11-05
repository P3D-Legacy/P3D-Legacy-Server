namespace P3D.Legacy.Common.Packets.Trade
{
    public sealed record TradeQuitPacket() : P3DPacket(P3DPacketType.TradeQuit)
    {
        public Origin DestinationPlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin destinationPlayerOrigin)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
        }
    }
}