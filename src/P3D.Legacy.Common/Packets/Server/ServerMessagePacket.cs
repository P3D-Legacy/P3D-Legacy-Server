namespace P3D.Legacy.Common.Packets.Server
{
    public class ServerMessagePacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.ServerMessage;

        public string Message { get => DataItems[0]; set => DataItems[0] = value; }
    }
}
