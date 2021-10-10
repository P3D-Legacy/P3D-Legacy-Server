namespace P3D.Legacy.Common.Packets.Chat
{
    public class ChatMessageGlobalPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.ChatMessageGlobal;

        public string Message { get => DataItems[0]; set => DataItems[0] = value; }
    }
}
