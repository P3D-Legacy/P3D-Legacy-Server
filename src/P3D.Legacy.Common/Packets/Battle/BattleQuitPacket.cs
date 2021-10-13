namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleQuitPacket() : P3DPacket(P3DPacketType.BattleQuit)
    {
        public Origin DestinationPlayerId { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin destinationPlayerId)
        {
            destinationPlayerId = DestinationPlayerId;
        }
    }
}