using System;
using System.Globalization;
using System.Text;

namespace P3D.Legacy.Common.Packets
{
    public abstract class P3DPacket
    {
        public abstract P3DPacketTypes Id { get; }

        public Origin Origin { get; set; }

        private static float _protocolVersion = 0.5f;
        public static float ProtocolVersion
        {
            get { return _protocolVersion; }
            set
            {
                _protocolVersion = value;
                ProtocolVersionString = ProtocolVersion.ToString(CultureInfo);
            }
        }
        public static string ProtocolVersionString { get; private set; } = ProtocolVersion.ToString(CultureInfo);

        public DataItems DataItems = new();

        protected static CultureInfo CultureInfo => CultureInfo.InvariantCulture;

        public static bool TryParseId(ReadOnlySpan<char> span, out P3DPacketTypes id)
        {
            var scanned = -1;
            var position = 0;


            id = 0;

            if (!span.Contains("|", StringComparison.Ordinal))
                return false;

            ParseChunk(ref span, ref scanned, ref position); // skip first
            var result = int.TryParse(ParseChunk(ref span, ref scanned, ref position), out var idInt);
            id = (P3DPacketTypes) idInt;
            return result;
        }

        public bool TryParseData(ReadOnlySpan<char> span)
        {
            var scanned = -1;
            var position = 0;


            if (!ParseChunk(ref span, ref scanned, ref position).Equals(ProtocolVersionString, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!int.TryParse(ParseChunk(ref span, ref scanned, ref position), out _))
                return false;


            if (!int.TryParse(ParseChunk(ref span, ref scanned, ref position), out var origin))
                return false;
            else
                Origin = origin;

            if (!int.TryParse(ParseChunk(ref span, ref scanned, ref position), out var dataItemsCount))
                return false;

            Span<int> offsets = dataItemsCount * 4 < 1024 ? stackalloc int[dataItemsCount] : new int[dataItemsCount];

            //Count from 4th item to second last item. Those are the offsets.
            for (var i = 0; i < dataItemsCount; i++)
            {
                if (!int.TryParse(ParseChunk(ref span, ref scanned, ref position), out var offset))
                    return false;
                else
                    offsets[i] = offset;
            }

            //Set the datastring, its the last item in the list. If it contained any separators, they will get read here:
            scanned += position + 1;
            var dataString = span.Slice(scanned, span.Length - scanned);

            //Cutting the data:
            for (var i = 0; i < offsets.Length; i++)
            {
                var cOffset = offsets[i];
                var length = dataString.Length - cOffset;

                if (i < offsets.Length - 1)
                    length = offsets[i + 1] - cOffset;

                if (length < 0)
                    return false;

                if (cOffset + length > dataString.Length)
                    return false;

                DataItems.AddToEnd(dataString.Slice(cOffset, length));
            }

            return true;
        }
        private static ReadOnlySpan<char> ParseChunk(ref ReadOnlySpan<char> span, ref int scanned, ref int position)
        {
            scanned += position + 1;

            position = span.Slice(scanned, span.Length - scanned).IndexOf('|');
            if (position < 0)
            {
                position = span.Slice(scanned, span.Length - scanned).Length;
            }

            return span.Slice(scanned, position);
        }


        public string CreateData()
        {
            var dataItems = DataItems.ToArray();

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(ProtocolVersion.ToString(CultureInfo));
            stringBuilder.Append("|");
            stringBuilder.Append(((int) Id).ToString(CultureInfo));
            stringBuilder.Append("|");
            stringBuilder.Append(Origin.ToString(CultureInfo));

            if (dataItems.Length == 0)
            {
                stringBuilder.Append("|0|");
                return stringBuilder.ToString();
            }

            stringBuilder.Append("|");
            stringBuilder.Append(dataItems.Length.ToString(CultureInfo));
            stringBuilder.Append("|0|");

            var num = 0;
            for (var i = 0; i < dataItems.Length - 1; i++)
            {
                num += dataItems[i].Length;
                stringBuilder.Append(num);
                stringBuilder.Append("|");
            }

            foreach (var dataItem in dataItems)
                stringBuilder.Append(dataItem);

            return stringBuilder.ToString();
        }
    }
}
