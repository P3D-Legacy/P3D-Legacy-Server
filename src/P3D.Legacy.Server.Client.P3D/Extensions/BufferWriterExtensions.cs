using P3D.Legacy.Common;

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Text;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    internal static class BufferWriterExtensions
    {
        private static readonly Encoder ASCII = Encoding.ASCII.GetEncoder();

        public static void WriteItems(this IBufferWriter<byte> output, DataItemStorage storage)
        {
            foreach (var (_, dataItem, _, isLast) in storage.WithMetadata())
                ASCII.Convert(dataItem, output, flush: isLast, out _, out _);
        }

        public static void Write(this IBufferWriter<byte> output, in ReadOnlySpan<char> chars, Encoder encoder)
        {
            encoder.Convert(chars, output, flush: false, out _, out _);
        }

        public static void WriteAsText(this IBufferWriter<byte> output, long val)
        {
            const int maxDigitCount = 20;
            var span = output.GetSpan(maxDigitCount);
            Utf8Formatter.TryFormat(val, span, out var bytesWritten);
            output.Advance(bytesWritten);
        }

        public static void Write(this IBufferWriter<byte> output, Protocol protocol)
        {
            output.Write(protocol == ProtocolVersion.V1 ? Protocol.SequenceV1 : Protocol.SequenceInvalid);
        }

        public static void Write(this IBufferWriter<byte> output, Origin origin)
        {
            const int maxDigitCount = 20;
            var span = output.GetSpan(maxDigitCount);
            Utf8Formatter.TryFormat(origin, span, out var bytesWritten);
            output.Advance(bytesWritten);
        }

        public static void Write(this IBufferWriter<byte> output, byte val)
        {
            const int length = sizeof(byte);
            var span = output.GetSpan(length);
            span[0] = val;
            output.Advance(length);
        }

        public static void Write(this IBufferWriter<byte> output, int val)
        {
            const int length = sizeof(int);
            var span = output.GetSpan(length);
            var cast = MemoryMarshal.Cast<byte, int>(span);
            cast[0] = val;
            output.Advance(length);
        }

        public static void Write(this IBufferWriter<byte> output, long val)
        {
            const int length = sizeof(long);
            var span = output.GetSpan(length);
            var cast = MemoryMarshal.Cast<byte, long>(span);
            cast[0] = val;
            output.Advance(length);
        }
    }
}