using System;

namespace P3D.Legacy.Common
{
    public readonly struct GameJoltId : IEquatable<GameJoltId>
    {
        public static GameJoltId None { get; } = new(0);

        public static implicit operator GameJoltId(int value) => new((ulong) value);
        public static implicit operator GameJoltId(long value) => new((ulong) value);
        public static implicit operator GameJoltId(uint value) => new(value);
        public static implicit operator GameJoltId(ulong value) => new(value);
        public static implicit operator ulong(GameJoltId value) => value._value;

        public static bool operator ==(GameJoltId left, GameJoltId right) => left.Equals(right);
        public static bool operator !=(GameJoltId left, GameJoltId right) => !(left == right);

        public bool IsNone => this == None;

        private readonly ulong _value;
        private GameJoltId(ulong origin) => _value = origin;

        public override string ToString() => _value.ToString();

        public bool Equals(GameJoltId other) => _value == other._value;
        public override bool Equals(object? obj) => obj is GameJoltId other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_value);
    }
}