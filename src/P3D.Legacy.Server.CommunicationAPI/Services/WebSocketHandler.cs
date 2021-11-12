using MediatR;

using Nerdbank.Streams;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.CommunicationAPI.Models;
using P3D.Legacy.Server.CommunicationAPI.Utils;

using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Services
{
    public sealed class WebSocketHandler :
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<ServerMessageNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>,
        IDisposable,
        IAsyncDisposable
    {
        private class WebSocketPlayer : IPlayer
        {
            private readonly Func<string, CancellationToken, Task>? _kickCallbackAsync;

            public string ConnectionId { get; }
            public PlayerId Id { get; private set; }
            public Origin Origin { get; private set; }
            public string Name { get; }
            public GameJoltId GameJoltId => GameJoltId.None;
            public PermissionFlags Permissions { get; private set; } = PermissionFlags.User;
            public IPEndPoint IPEndPoint => new(IPAddress.Loopback, 0);

            public WebSocketPlayer(string botName, Func<string, CancellationToken, Task>? kickCallbackAsync)
            {
                ConnectionId = $"CON_{botName}";
                Name = $"<BOT> {botName}";
                _kickCallbackAsync = kickCallbackAsync;
            }

            public Task AssignIdAsync(PlayerId id, CancellationToken ct)
            {
                Id = id;
                return Task.CompletedTask;
            }

            public Task AssignOriginAsync(Origin origin, CancellationToken ct)
            {
                Origin = origin;
                return Task.CompletedTask;
            }

            public Task AssignPermissionsAsync(PermissionFlags permissions, CancellationToken ct)
            {
                Permissions = permissions;
                return Task.CompletedTask;
            }

            public async Task KickAsync(string reason, CancellationToken ct)
            {
                if (_kickCallbackAsync is not null)
                    await _kickCallbackAsync(reason, ct);
            }
        }

        private readonly Guid _id = Guid.NewGuid();
        private IPlayer? _bot;

        private readonly WebSocket _webSocket;
        private readonly IMediator _mediator;
        private readonly DefaultJsonSerializer _jsonSerializer;
        private readonly SequenceTextReader _sequenceTextReader = new();

        public WebSocketHandler(WebSocket webSocket, IMediator mediator, DefaultJsonSerializer jsonSerializer)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        public async Task ListenAsync(CancellationToken ct)
        {
            const int maxMessageSize = 1 * 1024 * 1024;
            var buffer = new byte[4 * 1024 * 1024];
            while (!ct.IsCancellationRequested)
            {
                await using var reader = new WebSocketMessageReaderStream(_webSocket, maxMessageSize);
                var payload = await _jsonSerializer.DeserializeAsync<RequestPayload?>(reader, ct);
                if (payload is not null)
                    await ProcessPayloadAsync(payload, ct);
            }
        }

        private async Task ProcessPayloadAsync(RequestPayload request, CancellationToken ct)
        {
            switch (request)
            {
                case RegisterBotRequestPayload(var botName, var uid):
                    if (_bot is not null)
                    {
                        await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new ErrorResponsePayload(1, "Bot initialized twice!", uid)), WebSocketMessageType.Text, true, ct);
                        return;
                    }

                    _bot = new WebSocketPlayer(botName, KickCallbackAsync);
                    await _mediator.Send(new PlayerInitializingCommand(_bot), ct);
                    await _mediator.Send(new PlayerReadyCommand(_bot), ct);
                    await _mediator.Publish(new PlayerUpdatedStateNotification(_bot), ct);
                    await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new SuccessResponsePayload(uid)), WebSocketMessageType.Text, true, ct);
                    break;

                case MessageRequestPayload(var sender, var message, var uid):
                    if (_bot is null)
                    {
                        await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new ErrorResponsePayload(2, "Bot not initialized!", uid)), WebSocketMessageType.Text, true, ct);
                        return;
                    }

                    await _mediator.Publish(new PlayerSentGlobalMessageNotification(_bot, $"{sender}: {message}"), ct);
                    await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new SuccessResponsePayload(uid)), WebSocketMessageType.Text, true, ct);
                    break;
            }
        }

        private async Task KickCallbackAsync(string reason, CancellationToken ct)
        {
            await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new KickedResponsePayload(reason)), WebSocketMessageType.Text, true, ct);
            await _webSocket.CloseAsync(WebSocketCloseStatus.Empty, "Kicked", ct);
        }

        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new PlayerJoinedResponsePayload(notification.Player.Name)), WebSocketMessageType.Text, true, ct);
        }

        public async Task Handle(PlayerLeftNotification notification, CancellationToken ct)
        {
            await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new PlayerLeftResponsePayload(notification.Name)), WebSocketMessageType.Text, true, ct);
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new PlayerSentGlobalMessageResponsePayload(notification.Player.Name, notification.Message)), WebSocketMessageType.Text, true, ct);
        }

        public async Task Handle(ServerMessageNotification notification, CancellationToken ct)
        {
            await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new ServerMessageResponsePayload(notification.Message)), WebSocketMessageType.Text, true, ct);
        }

        public async Task Handle(PlayerTriggeredEventNotification notification, CancellationToken ct)
        {
            await _webSocket.SendAsync(_jsonSerializer.SerializeToUtf8Bytes(new PlayerTriggeredEventResponsePayload(notification.Player.Name, notification.EventMessage)), WebSocketMessageType.Text, true, ct);
        }

        public override int GetHashCode() => _id.GetHashCode();

        public void Dispose()
        {
            _webSocket.Dispose();
            _sequenceTextReader.Dispose();

            if (_bot is not null)
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                _mediator.Send(new PlayerFinalizingCommand(_bot), CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        public async ValueTask DisposeAsync()
        {
            _webSocket.Dispose();
            _sequenceTextReader.Dispose();

            if (_bot is not null)
                await _mediator.Send(new PlayerFinalizingCommand(_bot), CancellationToken.None);
        }
    }
}