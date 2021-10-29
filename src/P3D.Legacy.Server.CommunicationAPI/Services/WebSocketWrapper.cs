using MediatR;

using Nerdbank.Streams;

using P3D.Legacy.Server.Abstractions.Notifications;

using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Services
{
    public sealed class WebSocketWrapper :
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeavedNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<ServerMessageNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>,
        IDisposable
    {
        private sealed record ResponsePayload(int Type, string Response);
        private sealed record ErrorResponsePayload(int Code, string Message);


        private readonly Guid _id = Guid.NewGuid();
        private readonly WebSocket _webSocket;
        private readonly SequenceTextReader _sequenceTextReader = new();

        public WebSocketWrapper(WebSocket webSocket)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        }

        public override int GetHashCode() => _id.GetHashCode();

        public async Task ListenAsync(CancellationToken ct)
        {
            var buffer = new byte[1024 * 4];
            while (!ct.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    _sequenceTextReader.Initialize(new ReadOnlySequence<byte>(buffer, 0, result.Count), Encoding.UTF8);
                    while (await _sequenceTextReader.ReadLineAsync() is { } line)
                    {
                        ;
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    if (result.CloseStatus is { } closeStatus)
                    {
                        await _webSocket.CloseAsync(closeStatus, result.CloseStatusDescription, ct);
                    }

                    break;
                }
            }
        }

        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            var msg = $"Player {notification.Player.Name} joined the server!";
            await _webSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ResponsePayload(1, msg)), WebSocketMessageType.Text, true, ct);
        }

        public async Task Handle(PlayerLeavedNotification notification, CancellationToken ct)
        {
            var msg = $"Player {notification.Name} left the server!";
            await _webSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ResponsePayload(2, msg)), WebSocketMessageType.Text, true, ct);
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            var msg = $"<{notification.Player.Name}> {notification.Message}";
            await _webSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ResponsePayload(3, msg)), WebSocketMessageType.Text, true, ct);
        }

        public async Task Handle(ServerMessageNotification notification, CancellationToken ct)
        {
            var msg = $"{notification.Message}";
            await _webSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ResponsePayload(4, msg)), WebSocketMessageType.Text, true, ct);
        }

        public async Task Handle(PlayerTriggeredEventNotification notification, CancellationToken ct)
        {
            var msg = $"The player {notification.Player.Name} {notification.EventMessage}";
            await _webSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ResponsePayload(5, msg)), WebSocketMessageType.Text, true, ct);
        }

        public void Dispose()
        {
            _webSocket.Dispose();
            _sequenceTextReader.Dispose();
        }
    }
}