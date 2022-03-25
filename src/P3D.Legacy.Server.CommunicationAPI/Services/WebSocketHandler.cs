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
using System.Text.Json;
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
        private readonly NotificationPublisher _notificationPublisher;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly SequenceTextReader _sequenceTextReader = new();
        private readonly CancellationTokenSource _cts = new();

        public WebSocketHandler(WebSocket webSocket, IMediator mediator, NotificationPublisher notificationPublisher, JsonSerializerOptions jsonSerializerOptions)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        private ValueTask CheckWebSocketStateAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (_webSocket.CloseStatus is not null)
                _cts.Cancel();
            return ValueTask.CompletedTask;
        }

        private async Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            await CheckWebSocketStateAsync(cancellationToken);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            var ct = cts.Token;

            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, ct);
        }

        private async Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            await CheckWebSocketStateAsync(cancellationToken);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            var ct = cts.Token;

            await _webSocket.CloseAsync(closeStatus, statusDescription, ct);
        }

        public async Task ListenAsync(CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            var ct = cts.Token;

            try
            {
                const int maxMessageSize = 1 * 1024 * 1024;
                while (!ct.IsCancellationRequested)
                {
                    await CheckWebSocketStateAsync(ct);

                    await using var reader = new WebSocketMessageReaderStream(_webSocket, maxMessageSize);
                    var payload = await JsonSerializer.DeserializeAsync<RequestPayload?>(reader, _jsonSerializerOptions, ct);
                    if (payload is not null)
                        await ProcessPayloadAsync(payload, ct);
                }
            }
            catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely) { }
        }

        private async Task ProcessPayloadAsync(RequestPayload request, CancellationToken ct)
        {
            switch (request)
            {
                case RegisterBotRequestPayload(var botName, var uid):
                    if (_bot is not null)
                    {
                        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ErrorResponsePayload(1, "Bot initialized twice!", uid), _jsonSerializerOptions), ct);
                        return;
                    }

                    _bot = new WebSocketPlayer(botName, KickCallbackAsync);
                    await _mediator.Send(new PlayerInitializingCommand(_bot), ct);
                    await _mediator.Send(new PlayerReadyCommand(_bot), ct);
                    await _notificationPublisher.Publish(new PlayerUpdatedStateNotification(_bot), ct);
                    await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new SuccessResponsePayload(uid), _jsonSerializerOptions), ct);
                    break;

                case MessageRequestPayload(var sender, var message, var uid):
                    if (_bot is null)
                    {
                        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ErrorResponsePayload(2, "Bot not initialized!", uid), _jsonSerializerOptions), ct);
                        return;
                    }

                    await _notificationPublisher.Publish(new PlayerSentGlobalMessageNotification(_bot, $"{sender}: {message}"), ct);
                    await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new SuccessResponsePayload(uid), _jsonSerializerOptions), ct);
                    break;
            }
        }

        private async Task KickCallbackAsync(string reason, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new KickedResponsePayload(reason), _jsonSerializerOptions), ct);
            await CloseAsync(WebSocketCloseStatus.Empty, "Kicked", ct);
        }

        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerJoinedResponsePayload(notification.Player.Name), _jsonSerializerOptions), ct);
        }

        public async Task Handle(PlayerLeftNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerLeftResponsePayload(notification.Name), _jsonSerializerOptions), ct);
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerSentGlobalMessageResponsePayload(notification.Player.Name, notification.Message), _jsonSerializerOptions), ct);
        }

        public async Task Handle(ServerMessageNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ServerMessageResponsePayload(notification.Message), _jsonSerializerOptions), ct);
        }

        public async Task Handle(PlayerTriggeredEventNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerTriggeredEventResponsePayload(notification.Player.Name, notification.EventMessage), _jsonSerializerOptions), ct);
        }

        public override int GetHashCode() => _id.GetHashCode();

        public void Dispose()
        {
            _cts.Cancel();
            _webSocket.Dispose();
            _sequenceTextReader.Dispose();

            if (_bot is not null)
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                _mediator.Send(new PlayerFinalizingCommand(_bot), CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _webSocket.Dispose();
            _sequenceTextReader.Dispose();

            if (_bot is not null)
                await _mediator.Send(new PlayerFinalizingCommand(_bot), CancellationToken.None);
        }
    }
}