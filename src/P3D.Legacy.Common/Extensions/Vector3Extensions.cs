using System.Globalization;
using System.Numerics;
using System.Text;

namespace P3D.Legacy.Common.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 FromP3DString(string @string, char gameSeparator)
        {
            var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = gameSeparator.ToString() };

            var data = @string.Split('|');

            if (data.Length != 3)
                return Vector3.Zero;

            var xb = float.TryParse(data[0], NumberStyles.AllowDecimalPoint, numberFormat, out var x);
            var yb = float.TryParse(data[1], NumberStyles.AllowDecimalPoint, numberFormat, out var y);
            var zb = float.TryParse(data[2], NumberStyles.AllowDecimalPoint, numberFormat, out var z);

            if (xb && yb && zb)
                return new Vector3(x * 1000 / 1000, y * 1000 / 1000, z * 1000 / 1000);
            else
                return Vector3.Zero;
        }
        public static string ToP3DString(this Vector3 vector3, char gameSeparator)
        {
            var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = gameSeparator.ToString() };

            var data = new StringBuilder();
            data.Append((vector3.X * 1000 / 1000).ToString(numberFormat));
            data.Append('|');
            data.Append((vector3.Y * 1000 / 1000).ToString(numberFormat));
            data.Append('|');
            data.Append((vector3.Z * 1000 / 1000).ToString(numberFormat));

            return data.ToString();
        }
    }
}