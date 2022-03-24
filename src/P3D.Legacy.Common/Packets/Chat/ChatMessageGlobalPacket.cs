namespace P3D.Legacy.Common.Packets.Chat
{
    public sealed partial record ChatMessageGlobalPacket() : P3DPacket(P3DPacketType.ChatMessageGlobal)
    {
        public string Message { get => DataItemStorage.Get(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out string message)
        {
            message = Message;
        }
    }
}