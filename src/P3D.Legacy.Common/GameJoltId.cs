using System;
using System.Globalization;

namespace P3D.Legacy.Common
{
    public readonly record struct GameJoltId : IEquatable<ulong>
    {
        public static GameJoltId Parse(string id) => new(ulong.Parse(id, CultureInfo.InvariantCulture));

        public static GameJoltId None { get; } = new(0);
        public static GameJoltId FromNumber(ulong gameJoltId) => new(gameJoltId);

        public static implicit operator ulong(GameJoltId value) => value._value;

        public static bool operator ==(GameJoltId left, ulong right) => left._value.Equals(right);
        public static bool operator !=(GameJoltId left, ulong right) => !(left == right);

        public bool IsNone => this == None;

        private readonly ulong _value;
        private GameJoltId(ulong origin) => _value = origin;

        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);

        public bool Equals(ulong other) => _value == other;
    }
}