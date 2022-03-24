using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed partial record BattleOfferPacket() : P3DPacket(P3DPacketType.BattleOffer)
    {
        [P3DPacketDataItem(0, DataItemType.Origin)]
        public Origin DestinationPlayerOrigin { get; set; }
        [P3DPacketDataItem(1, DataItemType.P3DData)]
        public BattleOfferData BattleData { get; set; }

        public void Deconstruct(out Origin destinationPlayerOrigin, out BattleOfferData battleData)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
            battleData = BattleData;
        }
    }
}