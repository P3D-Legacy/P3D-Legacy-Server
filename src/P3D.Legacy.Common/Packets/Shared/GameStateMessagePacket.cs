namespace P3D.Legacy.Common.Packets.Shared
{
    public class GameStateMessagePacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.GameStateMessage;

        public string EventMessage { get => DataItems[0]; set => DataItems[0] = value; }
    }
}
