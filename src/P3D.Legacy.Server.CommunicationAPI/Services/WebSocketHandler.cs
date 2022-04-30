using Microsoft.Extensions.Options;

using Nerdbank.Streams;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.CommunicationAPI.Models;
using P3D.Legacy.Server.CommunicationAPI.Utils;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Services
{
    public sealed partial class WebSocketHandler :
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<ServerMessageNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>,
        IDisposable,
        IAsyncDisposable
    {
        [SuppressMessage("Performance", "CA1812")]
        [JsonSerializable(typeof(RequestPayload))]
        [JsonSerializable(typeof(PlayerJoinedResponsePayload))]
        [JsonSerializable(typeof(PlayerLeftResponsePayload))]
        [JsonSerializable(typeof(PlayerSentGlobalMessageResponsePayload))]
        [JsonSerializable(typeof(ServerMessageResponsePayload))]
        [JsonSerializable(typeof(PlayerTriggeredEventResponsePayload))]
        [JsonSerializable(typeof(KickedResponsePayload))]
        [JsonSerializable(typeof(SuccessResponsePayload))]
        [JsonSerializable(typeof(ErrorResponsePayload))]
        internal partial class JsonContext : JsonSerializerContext { }

        private class WebSocketPlayer : IPlayer
        {
            private readonly Func<string, CancellationToken, Task>? _kickCallbackAsync;

            public string ConnectionId { get; }
            public PlayerId Id { get; private set; }
            public Origin Origin { get; private set; }
            public string Name { get; }
            public GameJoltId GameJoltId => GameJoltId.None;
            public PermissionTypes Permissions { get; private set; } = PermissionTypes.User;
            public IPEndPoint IPEndPoint => new(IPAddress.Loopback, 0);
            public PlayerState State => PlayerState.Initialized;

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

            public Task AssignPermissionsAsync(PermissionTypes permissions, CancellationToken ct)
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
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly JsonContext _jsonContext;
        private readonly SequenceTextReader _sequenceTextReader = new();
        private readonly CancellationTokenSource _cts = new();

        public WebSocketHandler(WebSocket webSocket, ICommandDispatcher commandDispatcher, INotificationDispatcher notificationDispatcher, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _notificationDispatcher = notificationDispatcher ?? throw new ArgumentNullException(nameof(notificationDispatcher));
            _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
            _jsonContext = new JsonContext(new JsonSerializerOptions(_jsonSerializerOptions));
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
                    // TODO: Json Source generator with discriminators
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    var payload = await JsonSerializer.DeserializeAsync<RequestPayload?>(reader, _jsonSerializerOptions, ct);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
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
                        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ErrorResponsePayload(1, "Bot initialized twice!", uid), _jsonContext.ErrorResponsePayload), ct);
                        return;
                    }

                    _bot = new WebSocketPlayer(botName, KickCallbackAsync);
                    await _commandDispatcher.DispatchAsync(new PlayerInitializingCommand(_bot), ct);
                    await _commandDispatcher.DispatchAsync(new PlayerReadyCommand(_bot), ct);
                    await _notificationDispatcher.DispatchAsync(new PlayerUpdatedStateNotification(_bot), ct);
                    await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new SuccessResponsePayload(uid), _jsonContext.SuccessResponsePayload), ct);
                    break;

                case MessageRequestPayload(var sender, var message, var uid):
                    if (_bot is null)
                    {
                        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ErrorResponsePayload(2, "Bot not initialized!", uid), _jsonContext.ErrorResponsePayload), ct);
                        return;
                    }

                    await _notificationDispatcher.DispatchAsync(new PlayerSentGlobalMessageNotification(_bot, $"{sender}: {message}"), ct);
                    await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new SuccessResponsePayload(uid), _jsonContext.SuccessResponsePayload), ct);
                    break;
            }
        }

        private async Task KickCallbackAsync(string reason, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new KickedResponsePayload(reason), _jsonContext.KickedResponsePayload), ct);
            await CloseAsync(WebSocketCloseStatus.Empty, "Kicked", ct);
        }

        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerJoinedResponsePayload(notification.Player.Name), _jsonContext.PlayerJoinedResponsePayload), ct);
        }

        public async Task Handle(PlayerLeftNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerLeftResponsePayload(notification.Name), _jsonContext.PlayerLeftResponsePayload), ct);
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerSentGlobalMessageResponsePayload(notification.Player.Name, notification.Message), _jsonContext.PlayerSentGlobalMessageResponsePayload), ct);
        }

        public async Task Handle(ServerMessageNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ServerMessageResponsePayload(notification.Message), _jsonContext.ServerMessageResponsePayload), ct);
        }

        public async Task Handle(PlayerTriggeredEventNotification notification, CancellationToken ct)
        {
            await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerTriggeredEventResponsePayload(notification.Player.Name, notification.Event), _jsonContext.PlayerTriggeredEventResponsePayload), ct);
        }

        public override int GetHashCode() => _id.GetHashCode();

        public void Dispose()
        {
            _cts.Cancel();
            _webSocket.Dispose();
            _sequenceTextReader.Dispose();

            if (_bot is not null)
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                _commandDispatcher.DispatchAsync(new PlayerFinalizingCommand(_bot), CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _cts.Dispose();
            _webSocket.Dispose();
            _sequenceTextReader.Dispose();

            if (_bot is not null)
                await _commandDispatcher.DispatchAsync(new PlayerFinalizingCommand(_bot), CancellationToken.None);
        }
    }
}