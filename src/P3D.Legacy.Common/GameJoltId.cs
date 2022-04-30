using System;
using System.Globalization;

namespace P3D.Legacy.Common
{
    public readonly struct GameJoltId : IEquatable<GameJoltId>, IEquatable<ulong>
    {
        public static GameJoltId Parse(string id) => new(ulong.Parse(id, CultureInfo.InvariantCulture));

        public static GameJoltId None { get; } = new(0);
        public static GameJoltId FromNumber(ulong gameJoltId) => new(gameJoltId);

        public static implicit operator ulong(GameJoltId value) => value._value;

        public static bool operator ==(GameJoltId left, GameJoltId right) => left.Equals(right);
        public static bool operator !=(GameJoltId left, GameJoltId right) => !(left == right);
        public static bool operator ==(GameJoltId left, ulong right) => left._value.Equals(right);
        public static bool operator !=(GameJoltId left, ulong right) => !(left == right);

        public bool IsNone => this == None;

        private readonly ulong _value;
        private GameJoltId(ulong origin) => _value = origin;

        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);

        public bool Equals(GameJoltId other) => _value == other._value;
        public bool Equals(ulong other) => _value == other;
        public override bool Equals(object? obj) => obj is GameJoltId other && Equals(other) || obj is ulong other2 && Equals(other2);

        public override int GetHashCode() => HashCode.Combine(_value);
    }
}