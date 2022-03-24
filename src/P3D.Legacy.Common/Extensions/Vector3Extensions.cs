using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace P3D.Legacy.Common.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 FromP3DString(ReadOnlySpan<char> chars, char gameSeparator)
        {
            var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = gameSeparator.ToString() };

            Span<int> indices = stackalloc int[2];
            var spanIdx = 0;
            var span = chars;
            while (span.IndexOf('|') is var idx && idx != -1)
            {
                if (spanIdx > 2)
                    return Vector3.Zero;

                indices[spanIdx] = spanIdx != 0 ? idx + indices[spanIdx - 1] + 1 : idx;
                span = span.Slice(idx + 1);
                spanIdx++;
            }
            if (spanIdx != 2)
                return Vector3.Zero;

            var xs = chars.Slice(0, indices[0]);
            var ys = chars.Slice(indices[0] + 1, indices[1] - indices[0] - 1);
            var zs = chars.Slice(indices[1] + 1);

            var xb = float.TryParse(xs, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, numberFormat, out var x);
            var yb = float.TryParse(ys, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, numberFormat, out var y);
            var zb = float.TryParse(zs, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, numberFormat, out var z);

            if (xb && yb && zb)
                return new Vector3(x, y, z);
            else
                return Vector3.Zero;
        }
        public static string ToP3DString(this Vector3 vector3, char gameSeparator)
        {
            var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = gameSeparator.ToString() };

            var data = new StringBuilder();
            data.Append(vector3.X.ToString(numberFormat));
            data.Append('|');
            data.Append(vector3.Y.ToString(numberFormat));
            data.Append('|');
            data.Append(vector3.Z.ToString(numberFormat));

            return data.ToString();
        }
    }
}