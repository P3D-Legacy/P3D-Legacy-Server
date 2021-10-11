using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleClientDataPacket() : P3DPacket(P3DPacketType.BattleClientData)
    {
        public int DestinationPlayerId { get => DataItemStorage.GetInt32(0); init => DataItemStorage.SetInt32(0, value); }
        public BattleClientData BattleData { get => new(DataItemStorage.Get(1)); init => DataItemStorage.Set(1, value.ToP3DString()); }

        public void Deconstruct(out int destinationPlayerId, out BattleClientData battleData)
        {
            destinationPlayerId = DestinationPlayerId;
            battleData = BattleData;
        }
    }
}