using P3D.Legacy.Common;

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text;

namespace P3D.Legacy.Server.Client.P3D.Packets
{
    public abstract partial record P3DPacket
    {
        private static readonly byte[] Separator = "|"u8.ToArray();

        private static ReadOnlySpan<byte> ParseSection(ref ReadOnlySequence<byte> sequence)
        {
            var reader = new SequenceReader<byte>(sequence);
            reader.TryReadTo(out ReadOnlySpan<byte> section, Separator);

            sequence = reader.UnreadSequence;
            return section;
        }

        public static bool TryParseProtocol(ref ReadOnlySequence<byte> sequence, out Protocol protocol)
        {
            var reader = new SequenceReader<byte>(sequence);
            if (!reader.TryReadTo(out ReadOnlySpan<byte> section, Separator))
            {
                protocol = default;
                return false;
            }
            sequence = reader.UnreadSequence;

            protocol = new Protocol(section);
            return true;
        }

        public static bool TryParseId(ref ReadOnlySequence<byte> sequence, out P3DPacketType id)
        {
            var reader = new SequenceReader<byte>(sequence);
            if (!reader.TryReadTo(out ReadOnlySpan<byte> section, Separator))
            {
                id = P3DPacketType.None;
                return false;
            }
            sequence = reader.UnreadSequence;

            if (!Utf8Parser.TryParse(section, out int idInt, out _))
            {
                id = P3DPacketType.None;
                return false;
            }

            id = (P3DPacketType) idInt;
            return true;
        }

        public bool TryPopulateData(ref ReadOnlySequence<byte> sequence)
        {
            if (!Utf8Parser.TryParse(ParseSection(ref sequence), out int origin, out _))
                return false;

            Origin = Origin.From(origin);


            if (!Utf8Parser.TryParse(ParseSection(ref sequence), out int dataItemsCount, out _))
                return false;

            using var heapMemory = dataItemsCount * 4 >= 512 ? MemoryPool<int>.Shared.Rent(dataItemsCount) : null;
            var offsets = heapMemory is not null ? heapMemory.Memory.Span : stackalloc int[dataItemsCount];

            //Count from 4th item to second last item. Those are the offsets.
            for (var i = 0; i < dataItemsCount; i++)
            {
                if (!Utf8Parser.TryParse(ParseSection(ref sequence), out int offset, out _))
                    return false;

                offsets[i] = offset;
            }

            //Cutting the data:
            for (var i = 0; i < dataItemsCount; i++)
            {
                var cOffset = offsets[i];
                var length = sequence.Length - cOffset;

                if (i < dataItemsCount - 1)
                    length = offsets[i + 1] - cOffset;

                if (length < 0)
                    return false;

                if (cOffset + length > sequence.Length)
                    return false;

                DataItemStorage.Set(DataItemStorage.Count, Encoding.UTF8.GetString(sequence.Slice(cOffset, length)));
            }

            return true;
        }
    }
}