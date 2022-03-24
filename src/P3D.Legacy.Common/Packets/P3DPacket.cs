using System;
using System.Buffers;

namespace P3D.Legacy.Common.Packets
{
    public enum DataItemType
    {
        Origin,
        Int64,
        UInt64,
        Int32,
        Bool,
        Char,
        P3DData,
        String,
        StringArray,
        Vector3,
    }
    public sealed class P3DPacketDataItemAttribute : Attribute
    {
        public Range Position { get; set; }
        public DataItemType DataItemType { get; set; }

        public P3DPacketDataItemAttribute(int position, DataItemType dataItemType)
        {
            Position = Range.StartAt(0);
            DataItemType = dataItemType;
        }
        public P3DPacketDataItemAttribute(int start, int end, DataItemType dataItemType)
        {
            if (dataItemType != DataItemType.StringArray)
                throw new ArgumentException("Invalid type!", nameof(dataItemType));

            Position = new Range(start, end);
            DataItemType = dataItemType;
        }
    }

    public abstract partial record P3DPacket(P3DPacketType Id)
    {
        private RawPacketData _rawPacketData = new(ProtocolEnum.V1, Id, default, new());

        public Protocol Protocol { get => _rawPacketData.Protocol; private set => _rawPacketData = _rawPacketData with { Protocol = value }; }
        public P3DPacketType Id => _rawPacketData.Id;
        public Origin Origin { get => _rawPacketData.Origin; set => _rawPacketData = _rawPacketData with { Origin = value }; }
        public DataItemStorage DataItemStorage { get => _rawPacketData.DataItems; private set => _rawPacketData = _rawPacketData with { DataItems = value }; }

        public abstract void WriteDataItems(IBufferWriter<byte> writer);
    }
}