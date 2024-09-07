using System;
using System.Buffers;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework.Protocols;

public interface IMessageReader<TMessage>
{
    bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out TMessage message);
}