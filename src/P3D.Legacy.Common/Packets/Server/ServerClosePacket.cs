namespace P3D.Legacy.Common.Packets.Server
{
    public class ServerClosePacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.ServerClose;

        public string Reason { get => DataItems[0]; set => DataItems[0] = value; }
    }
}
