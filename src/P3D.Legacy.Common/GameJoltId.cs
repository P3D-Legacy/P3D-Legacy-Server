using System;
using System.Globalization;

namespace P3D.Legacy.Common
{
    public readonly record struct GameJoltId(ulong Value) : IEquatable<ulong>
    {
        public static GameJoltId Parse(string id) => new(ulong.Parse(id, CultureInfo.InvariantCulture));

        public static GameJoltId None { get; } = new(0);
        public static GameJoltId FromNumber(ulong gameJoltId) => new(gameJoltId);

        public static implicit operator ulong(GameJoltId value) => value.Value;

        public static bool operator ==(GameJoltId left, ulong right) => left.Value.Equals(right);
        public static bool operator !=(GameJoltId left, ulong right) => !(left == right);

        public bool IsNone => this == None;

        private ulong Value { get; } = Value;

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        public bool Equals(ulong other) => Value == other;
    }
}