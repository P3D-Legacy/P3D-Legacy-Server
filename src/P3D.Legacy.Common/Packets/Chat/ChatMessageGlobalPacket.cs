namespace P3D.Legacy.Common.Packets.Chat
{
    public sealed partial record ChatMessageGlobalPacket() : P3DPacket(P3DPacketType.ChatMessageGlobal)
    {
        [P3DPacketDataItem(0, DataItemType.String)]
        public string Message { get; set; }

        public void Deconstruct(out string message)
        {
            message = Message;
        }
    }
}