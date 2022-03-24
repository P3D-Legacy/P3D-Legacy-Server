namespace P3D.Legacy.Common.Packets.Chat
{
    public sealed partial record ChatMessagePrivatePacket() : P3DPacket(P3DPacketType.ChatMessagePrivate)
    {
        [P3DPacketDataItem(0, DataItemType.String)]
        public string DestinationPlayerName { get; set; }
        [P3DPacketDataItem(1, DataItemType.String)]
        public string Message { get; set; }

        public void Deconstruct(out string destinationPlayerName, out string message)
        {
            destinationPlayerName = DestinationPlayerName;
            message = Message;
        }
    }
}