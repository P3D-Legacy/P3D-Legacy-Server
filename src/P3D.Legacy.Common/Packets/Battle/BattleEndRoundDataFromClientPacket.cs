using P3D.Legacy.Common.Data.P3DDatas;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleEndRoundDataFromClientPacket() : P3DPacket(P3DPacketType.BattleEndRoundData)
    {
        public Origin DestinationPlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }
        public BattleEndRoundData BattleData { get => new(DataItemStorage.Get(1)); init => DataItemStorage.Set(1, value.ToP3DString()); }

        public void Deconstruct(out Origin destinationPlayerOrigin, out BattleEndRoundData battleData)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
            battleData = BattleData;
        }
    }
}