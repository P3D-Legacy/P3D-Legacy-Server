namespace P3D.Legacy.Common.Packets.Server
{
    public sealed partial record DestroyPlayerPacket() : P3DPacket(P3DPacketType.DestroyPlayer)
    {
        [P3DPacketDataItem(0, DataItemType.Origin)]
        public Origin PlayerOrigin { get; set; }

        public void Deconstruct(out Origin playerOrigin)
        {
            playerOrigin = PlayerOrigin;
        }
    }
}