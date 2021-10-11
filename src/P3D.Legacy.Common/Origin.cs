using System;

namespace P3D.Legacy.Common
{
    public readonly struct Origin : IEquatable<Origin>
    {
        public static Origin Server => new(-1);

        public static implicit operator Origin(int value) => new(value);
        public static implicit operator Origin(long value) => new(value);
        public static implicit operator Origin(uint value) => new((int) value);
        public static implicit operator Origin(ulong value) => new((int) value);
        public static implicit operator long(Origin value) => value._value;

        public static bool operator ==(Origin left, Origin right) => left.Equals(right);
        public static bool operator !=(Origin left, Origin right) => !(left == right);

        public bool IsServer => this == Server;
        public bool IsPlayer => _value > 0;

        private readonly long _value;
        private Origin(long origin) => _value = origin;

        public override string ToString() => _value.ToString();

        public bool Equals(Origin other) => _value == other._value;
        public override bool Equals(object? obj) => obj is Origin other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_value);
    }
}