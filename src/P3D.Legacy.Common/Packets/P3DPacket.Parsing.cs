using P3D.Legacy.Common.Data;

using System;
using System.Buffers;

namespace P3D.Legacy.Common.Packets
{
    public abstract partial record P3DPacket
    {
        private static ReadOnlySpan<char> ParseSection(in ReadOnlySequence<char> sequence, ref SequencePosition position)
        {
            var reader = new SequenceReader<char>(sequence.Slice(position));
            reader.TryReadTo(out ReadOnlySpan<char> section, '|', true);

            position = reader.Position;
            return section;
        }

        public static bool TryParseProtocol(in ReadOnlySequence<char> sequence, ref SequencePosition position, out Protocol protocol)
        {
            var reader = new SequenceReader<char>(sequence.Slice(position));
            if (!reader.TryReadTo(out ReadOnlySpan<char> _, '|', false))
            {
                protocol = default;
                return false;
            }

            protocol = new Protocol(ParseSection(in sequence, ref position));
            return true;
        }

        public static bool TryParseId(in ReadOnlySequence<char> sequence, ref SequencePosition position, out P3DPacketType id)
        {
            var reader = new SequenceReader<char>(sequence.Slice(position));
            if (!reader.TryReadTo(out ReadOnlySpan<char> _, '|', false))
            {
                id = P3DPacketType.None;
                return false;
            }

            var result = int.TryParse(ParseSection(in sequence, ref position), out var idInt);

            id = (P3DPacketType) idInt;
            return result;
        }

        public bool TryPopulateData(in ReadOnlySequence<char> sequence, ref SequencePosition position)
        {
            if (!int.TryParse(ParseSection(in sequence, ref position), out var origin))
                return false;

            Origin = origin;


            if (!int.TryParse(ParseSection(in sequence, ref position), out var dataItemsCount))
                return false;

            var offsets = dataItemsCount * 4 < 1024 ? stackalloc int[dataItemsCount] : new int[dataItemsCount];

            //Count from 4th item to second last item. Those are the offsets.
            for (var i = 0; i < dataItemsCount; i++)
            {
                if (!int.TryParse(ParseSection(in sequence, ref position), out var offset))
                    return false;
                else
                    offsets[i] = offset;
            }

            var remainingSequence = sequence.Slice(position);

            //Cutting the data:
            for (var i = 0; i < offsets.Length; i++)
            {
                var cOffset = offsets[i];
                var length = remainingSequence.Length - cOffset;

                if (i < offsets.Length - 1)
                    length = offsets[i + 1] - cOffset;

                if (length < 0)
                    return false;

                if (cOffset + length > remainingSequence.Length)
                    return false;

                DataItemStorage.Set(DataItemStorage.Count, remainingSequence.Slice(cOffset, length).ToString());
            }

            return true;
        }
    }
}