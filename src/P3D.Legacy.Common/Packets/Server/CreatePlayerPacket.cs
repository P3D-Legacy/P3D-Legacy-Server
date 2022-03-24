namespace P3D.Legacy.Common.Packets.Server
{
    public sealed partial record CreatePlayerPacket() : P3DPacket(P3DPacketType.CreatePlayer)
    {
        [P3DPacketDataItem(0, DataItemType.Origin)]
        public Origin PlayerOrigin { get; set; }

        public void Deconstruct(out Origin playerOrigin)
        {
            playerOrigin = PlayerOrigin;
        }
    }
}