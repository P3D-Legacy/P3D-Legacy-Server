namespace P3D.Legacy.Common.Packets.Chat
{
    public class ChatMessagePrivatePacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.ChatMessagePrivate;

        public string DestinationPlayerName { get => DataItems[0]; set => DataItems[0] = value; }
        public string Message { get => DataItems[1]; set => DataItems[1] = value; }
    }
}
