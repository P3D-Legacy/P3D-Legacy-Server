using P3D.Legacy.Common.Data.P3DDatas;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleEndRoundDataToClientPacket() : P3DPacket(P3DPacketType.BattleEndRoundData)
    {
        public BattleEndRoundData BattleData { get => new(DataItemStorage.Get(0)); init => DataItemStorage.Set(0, value.ToP3DString()); }

        public void Deconstruct(out BattleEndRoundData battleData)
        {
            battleData = BattleData;
        }
    }
}