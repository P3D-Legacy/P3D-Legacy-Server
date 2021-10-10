namespace P3D.Legacy.Common.Packets.Server
{
    public class WorldDataPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.WorldData;

        public int Season { get => int.Parse(DataItems[0] == string.Empty ? 0.ToString() : DataItems[0]); set => DataItems[0] = value.ToString(); }
        public int Weather { get => int.Parse(DataItems[1] == string.Empty ? 0.ToString() : DataItems[1]); set => DataItems[1] = value.ToString(); }
        public string CurrentTime { get => DataItems[2]; set => DataItems[2] = value; }
    }
}
