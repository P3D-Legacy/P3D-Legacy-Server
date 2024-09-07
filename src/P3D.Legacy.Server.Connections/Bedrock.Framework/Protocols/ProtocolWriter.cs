using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework.Protocols;

public sealed class ProtocolWriter : IAsyncDisposable
{
    private readonly PipeWriter _writer;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    public ProtocolWriter(Stream stream) : this(PipeWriter.Create(stream)) { }

    public ProtocolWriter(PipeWriter writer)
    {
        _writer = writer;
        _semaphore = new SemaphoreSlim(1);
    }

    public async ValueTask WriteAsync<TWriteMessage>(IMessageWriter<TWriteMessage> writer, TWriteMessage protocolMessage, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            if (_disposed)
            {
                return;
            }

            writer.WriteMessage(protocolMessage, _writer);

            var result = await _writer.FlushAsync(ct).ConfigureAwait(false);

            if (result.IsCanceled)
            {
                throw new OperationCanceledException();
            }

            if (result.IsCompleted)
            {
                _disposed = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask WriteManyAsync<TWriteMessage>(IMessageWriter<TWriteMessage> writer, IEnumerable<TWriteMessage> protocolMessages, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            if (_disposed)
            {
                return;
            }

            foreach (var protocolMessage in protocolMessages)
            {
                writer.WriteMessage(protocolMessage, _writer);
            }

            var result = await _writer.FlushAsync(ct).ConfigureAwait(false);

            if (result.IsCanceled)
            {
                throw new OperationCanceledException();
            }

            if (result.IsCompleted)
            {
                _disposed = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }
        finally
        {
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }
}