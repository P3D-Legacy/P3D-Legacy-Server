using Microsoft;

using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Utils;

internal class WebSocketMessageReaderStream : Stream, IDisposableObservable
{
    private readonly WebSocket _webSocket;
    private readonly long _maxMessageSize;

    private long _messageSize;
    private bool _endOfMessageReceived;

    public bool IsDisposed { get; private set; }

    public override bool CanRead => !IsDisposed;
    public override bool CanSeek => false;
    public override bool CanWrite => !IsDisposed;
    public override long Length => throw ThrowDisposedOr(new NotSupportedException());
    public override long Position { get => throw ThrowDisposedOr(new NotSupportedException()); set => throw ThrowDisposedOr(new NotSupportedException()); }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketMessageReaderStream"/> class.
    /// </summary>
    /// <param name="webSocket">The web socket to wrap in a stream.</param>
    /// <param name="maxMessageSize"></param>
    public WebSocketMessageReaderStream(WebSocket webSocket, long maxMessageSize = -1)
    {
        Requires.NotNull(webSocket, nameof(webSocket));
        _webSocket = webSocket;
        _maxMessageSize = maxMessageSize;
    }

    /// <summary>
    /// Does nothing, since web sockets do not need to be flushed.
    /// </summary>
    public override void Flush() { }

    /// <summary>
    /// Does nothing, since web sockets do not need to be flushed.
    /// </summary>
    /// <param name="ct">An ignored cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task FlushAsync(CancellationToken ct) => Task.CompletedTask;

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Verify.NotDisposed(this);

        if (_endOfMessageReceived || _webSocket.CloseStatus is not null || (_maxMessageSize != -1 && _messageSize > _maxMessageSize))
        {
            return 0;
        }

        var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);
        _endOfMessageReceived = result.EndOfMessage;
        _messageSize += result.Count;
        return result.Count;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw ThrowDisposedOr(new NotSupportedException());

    public override void SetLength(long value) => throw ThrowDisposedOr(new NotSupportedException());

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default) => throw ThrowDisposedOr(new NotSupportedException());

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits

    public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();

    public override void Write(byte[] buffer, int offset, int count) => throw ThrowDisposedOr(new NotSupportedException());

#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }

    private Exception ThrowDisposedOr(Exception ex)
    {
        Verify.NotDisposed(this);
        throw ex;
    }
}