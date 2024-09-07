using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework.Protocols;

public sealed class ProtocolReader : IAsyncDisposable
{
    private readonly PipeReader _reader;
    private SequencePosition _examined;
    private SequencePosition _consumed;
    private ReadOnlySequence<byte> _buffer;
    private bool _isCanceled;
    private bool _isCompleted;
    private bool _hasMessage;
    private bool _disposed;

    public ProtocolReader(Stream stream) : this(PipeReader.Create(stream)) { }

    public ProtocolReader(PipeReader reader)
    {
        _reader = reader;
    }

    public ValueTask<ProtocolReadResult<TReadMessage?>> ReadAsync<TReadMessage>(IMessageReader<TReadMessage> reader, CancellationToken ct = default)
    {
        return ReadAsync(reader, maximumMessageSize: null, ct);
    }

    public ValueTask<ProtocolReadResult<TReadMessage?>> ReadAsync<TReadMessage>(IMessageReader<TReadMessage> reader, int maximumMessageSize, CancellationToken ct = default)
    {
        return ReadAsync(reader, (int?) maximumMessageSize, ct);
    }

    public ValueTask<ProtocolReadResult<TReadMessage?>> ReadAsync<TReadMessage>(IMessageReader<TReadMessage> reader, int? maximumMessageSize, CancellationToken ct = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ProtocolReader));
        }

        if (_hasMessage)
        {
            throw new InvalidOperationException($"{nameof(Advance)} must be called before calling {nameof(ReadAsync)}");
        }

        // If this is the very first read, then make it go async since we have no data
        if (_consumed.GetObject() == null)
        {
            return DoAsyncReadAsync(maximumMessageSize, reader, ct);
        }

        // We have a buffer, test to see if there's any message left in the buffer
        if (TryParseMessage(maximumMessageSize, reader, _buffer, out var protocolMessage))
        {
            _hasMessage = true;
            return new ValueTask<ProtocolReadResult<TReadMessage?>>(new ProtocolReadResult<TReadMessage?>(protocolMessage, _isCanceled, IsCompleted: false));
        }

        // We couldn't parse the message so advance the input so we can read
        _reader.AdvanceTo(_consumed, _examined);

        // Reset the state since we're done consuming this buffer
        _buffer = default;
        _consumed = default;
        _examined = default;

        if (!_isCompleted)
        {
            return DoAsyncReadAsync(maximumMessageSize, reader, ct);
        }

        _consumed = default;
        _examined = default;

        // If we're complete then short-circuit
        if (!_buffer.IsEmpty)
        {
            throw new InvalidDataException("Connection terminated while reading a message.");
        }

        return new ValueTask<ProtocolReadResult<TReadMessage?>>(new ProtocolReadResult<TReadMessage?>(default, _isCanceled, _isCompleted));
    }

    private async ValueTask<ProtocolReadResult<TReadMessage?>> DoAsyncReadAsync<TReadMessage>(int? maximumMessageSize, IMessageReader<TReadMessage> reader, CancellationToken cancellationToken)
    {
        while (true)
        {
            var readTask = _reader.ReadAsync(cancellationToken);
            ReadResult result;
            if (readTask.IsCompletedSuccessfully)
            {
                result = await readTask;
            }
            else
            {
                return await ContinueDoAsyncReadAsync(readTask, maximumMessageSize, reader, cancellationToken);
            }

            var (shouldContinue, hasMessage) = TrySetMessage(result, maximumMessageSize, reader, out var protocolReadResult);
            if (hasMessage)
            {
                return await new ValueTask<ProtocolReadResult<TReadMessage?>>(protocolReadResult);
            }

            if (!shouldContinue)
            {
                break;
            }
        }

        return await new ValueTask<ProtocolReadResult<TReadMessage?>>(new ProtocolReadResult<TReadMessage?>(default, _isCanceled, _isCompleted));
    }

    private async ValueTask<ProtocolReadResult<TReadMessage?>> ContinueDoAsyncReadAsync<TReadMessage>(ValueTask<ReadResult> readTask, int? maximumMessageSize, IMessageReader<TReadMessage> reader, CancellationToken cancellationToken)
    {
        while (true)
        {
            var result = await readTask;

            var (shouldContinue, hasMessage) = TrySetMessage(result, maximumMessageSize, reader, out var protocolReadResult);
            if (hasMessage)
            {
                return protocolReadResult;
            }

            if (!shouldContinue)
            {
                break;
            }

            readTask = _reader.ReadAsync(cancellationToken);
        }

        return new ProtocolReadResult<TReadMessage?>(default, _isCanceled, _isCompleted);
    }

    private (bool ShouldContinue, bool HasMessage) TrySetMessage<TReadMessage>(ReadResult result, int? maximumMessageSize, IMessageReader<TReadMessage> reader, out ProtocolReadResult<TReadMessage?> readResult)
    {
        _buffer = result.Buffer;
        _isCanceled = result.IsCanceled;
        _isCompleted = result.IsCompleted;
        _consumed = _buffer.Start;
        _examined = _buffer.End;

        if (_isCanceled)
        {
            readResult = default;
            return (false, false);
        }

        if (TryParseMessage(maximumMessageSize, reader, _buffer, out var protocolMessage))
        {
            _hasMessage = true;
            readResult = new ProtocolReadResult<TReadMessage?>(protocolMessage, _isCanceled, IsCompleted: false);
            return (false, true);
        }

        _reader.AdvanceTo(_consumed, _examined);

        // Reset the state since we're done consuming this buffer
        _buffer = default;
        _consumed = default;
        _examined = default;

        if (_isCompleted)
        {
            _consumed = default;
            _examined = default;

            if (!_buffer.IsEmpty)
            {
                throw new InvalidDataException("Connection terminated while reading a message.");
            }

            readResult = default;
            return (false, false);
        }

        readResult = default;
        return (true, false);
    }

    private bool TryParseMessage<TReadMessage>(int? maximumMessageSize, IMessageReader<TReadMessage> reader, in ReadOnlySequence<byte> buffer, out TReadMessage protocolMessage)
    {
        // No message limit, just parse and dispatch
        if (maximumMessageSize == null)
        {
            return reader.TryParseMessage(buffer, ref _consumed, ref _examined, out protocolMessage);
        }

        // We give the parser a sliding window of the default message size
        var maxMessageSize = maximumMessageSize.Value;

        var segment = buffer;
        var overLength = false;

        if (segment.Length > maxMessageSize)
        {
            segment = segment.Slice(segment.Start, maxMessageSize);
            overLength = true;
        }

        if (reader.TryParseMessage(segment, ref _consumed, ref _examined, out protocolMessage))
        {
            return true;
        }

        if (overLength)
        {
            throw new InvalidDataException($"The maximum message size of {maxMessageSize}B was exceeded.");
        }

        return false;
    }

    public void Advance()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ProtocolReader));
        }

        _isCanceled = false;

        if (!_hasMessage)
        {
            return;
        }

        _buffer = _buffer.Slice(_consumed);

        _hasMessage = false;
    }

    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return default;
    }
}