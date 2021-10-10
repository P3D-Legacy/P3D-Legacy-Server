namespace P3D.Legacy.Common.Packets.Server
{
    public class IDPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.ID;

        public uint PlayerID { get => uint.Parse(DataItems[0] == string.Empty ? 0U.ToString() : DataItems[0]); set => DataItems[0] = value.ToString(); }
    }
}
