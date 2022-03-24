using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed partial record BattleHostDataPacket() : P3DPacket(P3DPacketType.BattleHostData)
    {
        public Origin DestinationPlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }
        public BattleHostData BattleData { get => new(DataItemStorage.Get(1)); init => DataItemStorage.Set(1, value.ToP3DString()); }

        public void Deconstruct(out Origin destinationPlayerOrigin, out BattleHostData battleData)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
            battleData = BattleData;
        }
    }
}