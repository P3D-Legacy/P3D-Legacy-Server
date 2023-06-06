using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Client.P3D.Packets.Battle
{
    public sealed record BattleJoinPacket() : P3DPacket(P3DPacketType.BattleJoin)
    {
        public Origin DestinationPlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin destinationPlayerOrigin)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
        }
    }
}