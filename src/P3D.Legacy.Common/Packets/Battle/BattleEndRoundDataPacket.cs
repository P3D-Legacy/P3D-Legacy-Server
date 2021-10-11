using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleEndRoundDataPacket() : P3DPacket(P3DPacketType.BattleEndRoundData)
    {
        public int DestinationPlayerId { get => DataItemStorage.GetInt32(0); init => DataItemStorage.SetInt32(0, value); }
        public BattleEndRoundData BattleData { get => new(DataItemStorage.Get(1)); init => DataItemStorage.Set(1, value.ToP3DString()); }

        public void Deconstruct(out int destinationPlayerId, out BattleEndRoundData battleData)
        {
            destinationPlayerId = DestinationPlayerId;
            battleData = BattleData;
        }
    }
}