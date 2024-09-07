using Microsoft.Extensions.Options;

using Nerdbank.Streams;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.CommunicationAPI.Models;
using P3D.Legacy.Server.CommunicationAPI.Utils;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;

using System;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Services;

public sealed partial class WebSocketHandler :
    IEventHandler<PlayerJoinedEvent>,
    IEventHandler<PlayerLeftEvent>,
    IEventHandler<PlayerSentGlobalMessageEvent>,
    IEventHandler<ServerMessageEvent>,
    IEventHandler<PlayerTriggeredEventEvent>,
    IDisposable,
    IAsyncDisposable
{
    [JsonSerializable(typeof(RequestPayload))]
    [JsonSerializable(typeof(ResponsePayload))]
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
    internal partial class JsonContext : JsonSerializerContext { }

    private class WebSocketPlayer : IPlayer
    {
        private readonly Func<string, CancellationToken, Task>? _kickCallbackAsync;

        public string ConnectionId { get; }
        public PlayerId Id { get; private set; }
        public Origin Origin { get; private set; }
        public string Name { get; }
        public PermissionTypes Permissions { get; private set; } = PermissionTypes.User;
        public IPEndPoint IPEndPoint => new(IPAddress.Loopback, 0);
        public PlayerState State { get; internal set; } = PlayerState.None;

        public WebSocketPlayer(string botName, Func<string, CancellationToken, Task>? kickCallbackAsync)
        {
            ConnectionId = $"CON_{botName}";
            Name = $"<BOT> {botName}";
            _kickCallbackAsync = kickCallbackAsync;
        }

        public Task<GameJoltId> GetGameJoltIdOrNoneAsync(CancellationToken ct) => Task.FromResult(GameJoltId.None);

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
    private WebSocketPlayer? _bot;

    private readonly WebSocket _webSocket;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly JsonContext _jsonContext;
    private readonly SequenceTextReader _sequenceTextReader = new();
    private readonly CancellationTokenSource _cts = new();

    public WebSocketHandler(WebSocket webSocket, ICommandDispatcher commandDispatcher, IEventDispatcher eventDispatcher, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _jsonContext = new JsonContext(new JsonSerializerOptions(jsonSerializerOptions.Value));
    }

    private async ValueTask CheckWebSocketStateAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (_webSocket.CloseStatus is not null)
            await _cts.CancelAsync();
    }

    private async Task SendAsync(ArraySegment<byte> buffer, CancellationToken ct)
    {
        await CheckWebSocketStateAsync(ct);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, ct);
        var ct2 = cts.Token;

        try
        {
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, ct2);
        }
        catch (OperationCanceledException) { }
    }

    private async Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken ct)
    {
        await CheckWebSocketStateAsync(ct);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, ct);
        var ct2 = cts.Token;

        try
        {
            await _webSocket.CloseAsync(closeStatus, statusDescription, ct2);
        }
        catch (OperationCanceledException) { }
    }

    public async Task ListenAsync(CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, ct);
        var ct2 = cts.Token;

        try
        {
            const int maxMessageSize = 1 * 1024 * 1024;
            while (!ct2.IsCancellationRequested)
            {
                await CheckWebSocketStateAsync(ct2);

                await using var reader = new WebSocketMessageReaderStream(_webSocket, maxMessageSize);
                var payload = await JsonSerializer.DeserializeAsync<RequestPayload>(reader, _jsonContext.RequestPayload, ct2);
                if (payload is not null)
                    await ProcessPayloadAsync(payload, ct2);
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely) { }
    }

    private async Task ProcessPayloadAsync(RequestPayload request, CancellationToken ct)
    {
        switch (request)
        {
            case RegisterBotRequestPayload(var botName, var uid):
                if (_bot is not null)
                {
                    // Explore the idea of writing text messages via Stream to avoid buffer usage
                    await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ErrorResponsePayload(1, "Bot initialized twice!", uid), _jsonContext.ResponsePayload), ct);
                    return;
                }

                _bot = new WebSocketPlayer(botName, KickCallbackAsync);
                _bot.State = PlayerState.Initializing;
                await _commandDispatcher.DispatchAsync(new PlayerInitializingCommand(_bot), ct);
                _bot.State = PlayerState.Authentication;
                await _commandDispatcher.DispatchAsync(new PlayerReadyCommand(_bot), ct);
                _bot.State = PlayerState.Initialized;
                await _eventDispatcher.DispatchAsync(new PlayerUpdatedStateEvent(_bot), ct);
                await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new SuccessResponsePayload(uid), _jsonContext.ResponsePayload), ct);
                break;

            case MessageRequestPayload(var sender, var message, var uid):
                if (_bot is null)
                {
                    await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ErrorResponsePayload(2, "Bot not initialized!", uid), _jsonContext.ResponsePayload), ct);
                    return;
                }

                await _eventDispatcher.DispatchAsync(new PlayerSentGlobalMessageEvent(_bot, $"{sender}: {message}"), ct);
                await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new SuccessResponsePayload(uid), _jsonContext.ResponsePayload), ct);
                break;
        }
    }

    private async Task KickCallbackAsync(string reason, CancellationToken ct)
    {
        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new KickedResponsePayload(reason), _jsonContext.ResponsePayload), ct);
        await CloseAsync(WebSocketCloseStatus.Empty, "Kicked", ct);
    }

    public async Task HandleAsync(IReceiveContext<PlayerJoinedEvent> context, CancellationToken ct)
    {
        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerJoinedResponsePayload(context.Message.Player.Name), _jsonContext.ResponsePayload), ct);
    }

    public async Task HandleAsync(IReceiveContext<PlayerLeftEvent> context, CancellationToken ct)
    {
        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerLeftResponsePayload(context.Message.Name), _jsonContext.ResponsePayload), ct);
    }

    public async Task HandleAsync(IReceiveContext<PlayerSentGlobalMessageEvent> context, CancellationToken ct)
    {
        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerSentGlobalMessageResponsePayload(context.Message.Player.Name, context.Message.Message), _jsonContext.ResponsePayload), ct);
    }

    public async Task HandleAsync(IReceiveContext<ServerMessageEvent> context, CancellationToken ct)
    {
        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new ServerMessageResponsePayload(context.Message.Message), _jsonContext.ResponsePayload), ct);
    }

    public async Task HandleAsync(IReceiveContext<PlayerTriggeredEventEvent> context, CancellationToken ct)
    {
        await SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PlayerTriggeredEventResponsePayload(context.Message.Player.Name, context.Message.Event), _jsonContext.ResponsePayload), ct);
    }

    public override int GetHashCode() => _id.GetHashCode();

    public void Dispose()
    {
        _cts.Cancel();
        _sequenceTextReader.Dispose();

        if (_bot is not null)
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            _commandDispatcher.DispatchAsync(new PlayerFinalizingCommand(_bot), CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
        _sequenceTextReader.Dispose();

        if (_bot is not null)
            await _commandDispatcher.DispatchAsync(new PlayerFinalizingCommand(_bot), CancellationToken.None);
    }
}