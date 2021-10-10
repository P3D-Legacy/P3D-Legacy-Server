using System;
using System.Globalization;

namespace P3D.Legacy.Common
{
    public readonly struct Origin : IEquatable<Origin>
    {
        public static Origin Server => new(-1);

        public static implicit operator Origin(int origin) => new(origin);
        public static implicit operator Origin(uint origin) => new((int) origin);
        public static implicit operator int(Origin origin) => origin._value;

        public static bool operator ==(Origin left, Origin right) => left.Equals(right);
        public static bool operator !=(Origin left, Origin right) => !(left == right);

        public bool IsServer => this == Server;

        private readonly int _value;
        private Origin(int origin) => _value = origin;

        public override string ToString() => _value.ToString();
        public string ToString(CultureInfo cultureInfo) => _value.ToString(cultureInfo);

        public bool Equals(Origin other) => _value == other._value;
        public override bool Equals(object obj) => obj is Origin other && Equals(other);

        public override int GetHashCode() => _value;
    }
}