using P3D.Legacy.Server.Client.P3D.Data.P3DDatas;

namespace P3D.Legacy.Server.Client.P3D.Packets.Battle
{
    public sealed record BattleHostDataToClientPacket() : P3DPacket(P3DPacketType.BattleHostData)
    {
        public BattleHostData BattleData { get => new(DataItemStorage.Get(0)); init => DataItemStorage.Set(0, value.ToP3DString()); }

        public void Deconstruct(out BattleHostData battleData)
        {
            battleData = BattleData;
        }
    }
}