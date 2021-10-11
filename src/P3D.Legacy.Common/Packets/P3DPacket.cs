using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Common.Packets
{
    public abstract partial record P3DPacket(P3DPacketType Id)
    {
        private RawPacketData _rawPacketData = new(ProtocolEnum.V1, Id, default, new());

        public Protocol Protocol { get => _rawPacketData.Protocol; private set => _rawPacketData = _rawPacketData with { Protocol = value }; }
        public P3DPacketType Id => _rawPacketData.Id;
        public Origin Origin { get => _rawPacketData.Origin; set => _rawPacketData = _rawPacketData with { Origin = value }; }
        public DataItemStorage DataItemStorage { get => _rawPacketData.DataItems; private set => _rawPacketData = _rawPacketData with { DataItems = value }; }

        public string CreateData() => _rawPacketData.Build();
    }
}