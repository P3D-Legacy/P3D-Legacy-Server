namespace P3D.Legacy.Common.Packets.Server
{
    public class KickedPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.Kicked;

        public string Reason { get => DataItems[0]; set => DataItems[0] = value; }
    }
}
