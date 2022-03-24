namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed partial record BattleJoinPacket() : P3DPacket(P3DPacketType.BattleJoin)
    {
        public Origin DestinationPlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin destinationPlayerOrigin)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
        }
    }
}