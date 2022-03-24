namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed partial record BattleJoinPacket() : P3DPacket(P3DPacketType.BattleJoin)
    {
        [P3DPacketDataItem(0, DataItemType.Origin)]
        public Origin DestinationPlayerOrigin { get; set; }

        public void Deconstruct(out Origin destinationPlayerOrigin)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
        }
    }
}