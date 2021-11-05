namespace P3D.Legacy.Common.Packets.Chat
{
    public sealed record ChatMessagePrivatePacket() : P3DPacket(P3DPacketType.ChatMessagePrivate)
    {
        public string DestinationPlayerName { get => DataItemStorage.Get(1); init => DataItemStorage.Set(1, value); }
        public string Message { get => DataItemStorage.Get(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out string destinationPlayerName, out string message)
        {
            destinationPlayerName = DestinationPlayerName;
            message = Message;
        }
    }
}