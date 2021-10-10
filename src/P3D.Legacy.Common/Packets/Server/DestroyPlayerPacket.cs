namespace P3D.Legacy.Common.Packets.Server
{
    public class DestroyPlayerPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.DestroyPlayer;

        public int PlayerID { get => int.Parse(DataItems[0] == string.Empty ? 0.ToString() : DataItems[0]); set => DataItems[0] = value.ToString(); }
    }
}
