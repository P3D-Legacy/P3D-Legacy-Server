using System;

namespace P3D.Legacy.Common
{
    public enum ProtocolEnum : byte { Invalid, V1 }

    public readonly struct Protocol : IEquatable<Protocol>
    {
        private static readonly ReadOnlyMemory<char> SequenceV1 = "0.5".AsMemory();

        public static implicit operator Protocol(ProtocolEnum value) => new(value);
        public static implicit operator ProtocolEnum(Protocol value) => value._value;

        public static bool operator ==(Protocol left, Protocol right) => left.Equals(right);
        public static bool operator !=(Protocol left, Protocol right) => !(left == right);

        private readonly ProtocolEnum _value;

        public Protocol(in ReadOnlySpan<char> protocol)
        {
            if (protocol.SequenceEqual(SequenceV1.Span))
                _value = ProtocolEnum.V1;
            else
            {
                _value = ProtocolEnum.Invalid;
            }
        }
        public Protocol(ProtocolEnum value)
        {
            _value = value;
        }

        public override string ToString() => _value switch
        {
            ProtocolEnum.V1 => "0.5",
            _ => "0.0",
        };

        public bool Equals(Protocol other) => _value == other._value;
        public override bool Equals(object? obj) => obj is Protocol other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_value);
    }
}