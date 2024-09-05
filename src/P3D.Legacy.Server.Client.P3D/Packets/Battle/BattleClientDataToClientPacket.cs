using P3D.Legacy.Common;
using P3D.Legacy.Server.Client.P3D.Data.P3DDatas;

namespace P3D.Legacy.Server.Client.P3D.Packets.Battle;

public sealed record BattleClientDataToClientPacket() : P3DPacket(P3DPacketType.BattleClientData)
{
    public BattleClientData BattleData { get => new(DataItemStorage.Get(0)); init => DataItemStorage.Set(0, value.ToP3DString()); }

    public void Deconstruct(out BattleClientData battleData)
    {
        battleData = BattleData;
    }
}