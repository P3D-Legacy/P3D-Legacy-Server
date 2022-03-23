using System;
using System.Buffers;
using System.Text;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    internal static class BufferWriterExtensions
    {
        public static void Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> chars, Encoder encoder)
        {
            encoder.Convert(chars, writer, false, out _, out _);
        }
        public static void Write(this IBufferWriter<byte> writer, byte val, Encoder encoder)
        {
            encoder.Convert(val.ToString(), writer, false, out _, out _);
        }
        public static void Write(this IBufferWriter<byte> writer, int val, Encoder encoder)
        {
            encoder.Convert(val.ToString(), writer, false, out _, out _);
        }
    }
}