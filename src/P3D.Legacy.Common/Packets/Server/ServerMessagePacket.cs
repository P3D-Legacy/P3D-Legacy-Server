namespace P3D.Legacy.Common.Packets.Server
{
    public sealed partial record ServerMessagePacket() : P3DPacket(P3DPacketType.ServerMessage)
    {
        [P3DPacketDataItem(0, DataItemType.String)]
        public string Message { get; set; }

        public void Deconstruct(out string message)
        {
            message = Message;
        }
    }
}