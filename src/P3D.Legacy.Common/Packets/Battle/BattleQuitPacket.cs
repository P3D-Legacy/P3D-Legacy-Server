namespace P3D.Legacy.Common.Packets.Battle
{
    public class BattleQuitPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.BattleQuit;

        public int DestinationPlayerID { get => int.Parse(DataItems[0] == string.Empty ? 0.ToString() : DataItems[0]); set => DataItems[0] = value.ToString(); }
    }
}
