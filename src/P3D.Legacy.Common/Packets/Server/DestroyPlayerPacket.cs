namespace P3D.Legacy.Common.Packets.Server
{
    public sealed record DestroyPlayerPacket() : P3DPacket(P3DPacketType.DestroyPlayer)
    {
        public ulong PlayerId { get => DataItemStorage.GetUInt64(0); init => DataItemStorage.SetUInt64(0, value); }

        public void Deconstruct(out ulong playerId)
        {
            playerId = PlayerId;
        }
    }
}