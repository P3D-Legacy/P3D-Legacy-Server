using System;
using System.Globalization;

namespace P3D.Legacy.Common
{
    public readonly struct Origin : IEquatable<Origin>, IComparable<Origin>
    {
        public static Origin Parse(string id) => new(long.Parse(id, CultureInfo.InvariantCulture));

        public static Origin None { get; } = new(0);
        public static Origin Server { get; } = new(-1);
        public static Origin FromNumber(long origin) => new(origin);

        public static implicit operator long(Origin value) => value._value;

        public bool IsNone => this == None;
        public bool IsServer => this == Server;
        public bool IsPlayer => _value > 0;

        private readonly long _value;
        private Origin(long origin) => _value = origin;

        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);

        public override int GetHashCode() => HashCode.Combine(_value);

        public bool Equals(Origin other) => _value == other._value;
        public override bool Equals(object? obj) => obj is Origin other && Equals(other);
        public static bool operator ==(Origin left, Origin right) => left.Equals(right);
        public static bool operator !=(Origin left, Origin right) => !(left == right);


        public int CompareTo(Origin other) => _value.CompareTo(other._value);
        public static bool operator <(Origin left, Origin right) => left.CompareTo(right) < 0;
        public static bool operator >(Origin left, Origin right) => left.CompareTo(right) > 0;
        public static bool operator <=(Origin left, Origin right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Origin left, Origin right) => left.CompareTo(right) >= 0;
    }
}