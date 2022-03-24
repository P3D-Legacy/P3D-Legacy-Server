namespace P3D.Legacy.Common.Packets.Shared
{
    public sealed partial record GameStateMessagePacket() : P3DPacket(P3DPacketType.GameStateMessage)
    {
        [P3DPacketDataItem(0, DataItemType.String)]
        public string EventMessage { get; set; }

        public void Deconstruct(out string eventMessage)
        {
            eventMessage = EventMessage;
        }
    }
}