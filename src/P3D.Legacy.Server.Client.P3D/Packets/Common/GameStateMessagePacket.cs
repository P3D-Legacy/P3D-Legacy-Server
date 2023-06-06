namespace P3D.Legacy.Server.Client.P3D.Packets.Common
{
    public sealed record GameStateMessagePacket() : P3DPacket(P3DPacketType.GameStateMessage)
    {
        public string EventMessage { get => DataItemStorage.Get(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out string eventMessage)
        {
            eventMessage = EventMessage;
        }
    }
}