using System;

namespace P3D.Legacy.Server.Client.P3D;

public enum ProtocolVersion { Invalid, V1, }

public readonly record struct Protocol
{
    public static readonly byte[] SequenceInvalid = "0.0"u8.ToArray();
    public static readonly byte[] SequenceV1 = "0.5"u8.ToArray();

    public static implicit operator Protocol(ProtocolVersion value) => new(value);
    public static implicit operator ProtocolVersion(Protocol value) => value._value;


    private readonly ProtocolVersion _value;

    public Protocol(in ReadOnlySpan<byte> protocol)
    {
        _value = protocol.SequenceEqual(SequenceV1) ? ProtocolVersion.V1 : ProtocolVersion.Invalid;
    }
    public Protocol(ProtocolVersion value)
    {
        _value = value;
    }

    public override string ToString() => _value switch
    {
        ProtocolVersion.V1 => "0.5",
        _ => "0.0",
    };
}