using System;
using System.Globalization;

namespace P3D.Legacy.Server.Abstractions.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class BuildDateTimeAttribute : Attribute
    {
        public DateTime DateTime { get; }

        public BuildDateTimeAttribute(string value)
        {
            DateTime = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
    }
}