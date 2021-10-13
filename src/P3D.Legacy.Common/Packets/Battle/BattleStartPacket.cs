namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleStartPacket() : P3DPacket(P3DPacketType.BattleStart)
    {
        public Origin DestinationPlayerId { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin destinationPlayerId)
        {
            destinationPlayerId = DestinationPlayerId;
        }
    }
}