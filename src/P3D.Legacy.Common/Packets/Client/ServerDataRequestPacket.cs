namespace P3D.Legacy.Common.Packets.Client
{
    public class ServerDataRequestPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.ServerDataRequest;

        public string DontEvenKnow { get => DataItems[0]; set => DataItems[0] = value; }
    }
}
