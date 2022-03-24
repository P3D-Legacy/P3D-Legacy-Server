namespace P3D.Legacy.Common.Packets.Server
{
    public sealed partial record ServerClosePacket() : P3DPacket(P3DPacketType.ServerClose)
    {
        [P3DPacketDataItem(0, DataItemType.String)]
        public string Reason { get; set; }

        public void Deconstruct(out string reason)
        {
            reason = Reason;
        }
    }
}