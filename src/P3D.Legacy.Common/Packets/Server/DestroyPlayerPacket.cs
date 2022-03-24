namespace P3D.Legacy.Common.Packets.Server
{
    public sealed partial record DestroyPlayerPacket() : P3DPacket(P3DPacketType.DestroyPlayer)
    {
        public Origin PlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin playerOrigin)
        {
            playerOrigin = PlayerOrigin;
        }
    }
}