namespace P3D.Legacy.Common.Packets.Server
{
    public sealed partial record KickedPacket() : P3DPacket(P3DPacketType.Kicked)
    {
        [P3DPacketDataItem(0, DataItemType.String)]
        public string Reason { get; set; }

        public void Deconstruct(out string reason)
        {
            reason = Reason;
        }
    }
}