using System;
using System.Globalization;

namespace P3D.Legacy.Common
{
    public readonly record struct Origin(long Value) : IComparable<Origin>
    {
        public static Origin Parse(string origin) => new(long.Parse(origin, CultureInfo.InvariantCulture));

        public static Origin None { get; } = new(0);
        public static Origin Server { get; } = new(-1);
        public static Origin FromNumber(long origin) => new(origin);

        public static implicit operator long(Origin value) => value.Value;

        public bool IsNone => this == None;
        public bool IsServer => this == Server;
        public bool IsPlayer => Value > 0;

        private long Value { get; } = Value;

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        public int CompareTo(Origin other) => Value.CompareTo(other.Value);
        public static bool operator <(Origin left, Origin right) => left.CompareTo(right) < 0;
        public static bool operator >(Origin left, Origin right) => left.CompareTo(right) > 0;
        public static bool operator <=(Origin left, Origin right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Origin left, Origin right) => left.CompareTo(right) >= 0;
    }
}