namespace P3D.Legacy.Common.Packets.Server
{
    public sealed record ServerClosePacket() : P3DPacket(P3DPacketType.ServerClose)
    {
        public string Reason { get => DataItemStorage.Get(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out string reason)
        {
            reason = Reason;
        }
    }
}