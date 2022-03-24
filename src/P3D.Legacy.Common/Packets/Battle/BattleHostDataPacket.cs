using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed partial record BattleHostDataPacket() : P3DPacket(P3DPacketType.BattleHostData)
    {
        [P3DPacketDataItem(0, DataItemType.Origin)]
        public Origin DestinationPlayerOrigin { get; set; }
        [P3DPacketDataItem(1, DataItemType.P3DData)]
        public BattleHostData BattleData { get; set; }

        public void Deconstruct(out Origin destinationPlayerOrigin, out BattleHostData battleData)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
            battleData = BattleData;
        }
    }
}