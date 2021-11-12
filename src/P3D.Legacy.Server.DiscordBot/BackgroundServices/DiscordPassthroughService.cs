using BetterHostedServices;

using Discord;
using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.DiscordBot.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.DiscordBot.BackgroundServices
{
    internal sealed class DiscordPassthroughService : CriticalBackgroundService,
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<ServerMessageNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMediator _mediator;
        private readonly DiscordSocketClient? _discordSocketClient;
        private readonly DiscordOptions _options;

        public DiscordPassthroughService(
            ILogger<DiscordPassthroughService> logger,
            TracerProvider traceProvider,
            DiscordSocketClient discordSocketClient,
            IServiceScopeFactory scopeFactory,
            IOptions<DiscordOptions> options,
            IMediator mediator,
            IApplicationEnder applicationEnder) : base(applicationEnder)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.DiscordBot");
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _discordSocketClient = options.Value.PasstroughChannelId != 0
                ? discordSocketClient ?? throw new ArgumentNullException(nameof(discordSocketClient))
                : null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var span = _tracer.StartActiveSpan("Discord Bot");

            async void OnCancellation(object? _, CancellationToken ct)
            {
                ct.ThrowIfCancellationRequested();
                _discordSocketClient.MessageReceived -= BotMessageReceivedAsync;
                _discordSocketClient.Log -= BotLogAsync;
                await _discordSocketClient.StopAsync();
                _logger.LogWarning("Stopped Discord Bot");
            }

            if (_discordSocketClient is null)
                return;

            using var scope = _scopeFactory.CreateScope();

            if (_discordSocketClient.ConnectionState != ConnectionState.Disconnecting && _discordSocketClient.ConnectionState != ConnectionState.Disconnected)
                return;

            var botToken = _options.BotToken;
            if (string.IsNullOrEmpty(botToken))
            {
                _logger.LogError("Error while getting Discord.BotToken! Check your configuration file");
                return;
            }

            _discordSocketClient.MessageReceived += BotMessageReceivedAsync;
            _discordSocketClient.Log += BotLogAsync;

            await _discordSocketClient.LoginAsync(TokenType.Bot, botToken);
            await _discordSocketClient.StartAsync();

            _logger.LogWarning("Started Discord Bot");

#if NET5_0
            stoppingToken.Register(_ => OnCancellation(null, stoppingToken), null);
#else
            stoppingToken.Register(OnCancellation, null);
#endif
        }

        private Task BotLogAsync(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(arg.Exception, "Critical log entry: {Message}", arg.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(arg.Exception, "Error log entry: {Message}", arg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(arg.Exception, "Warning log entry: {Message}", arg.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(arg.Exception, "Info log entry: {Message}", arg.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(arg.Exception, "Verbose log entry: {Message}", arg.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(arg.Exception, "Debug log entry: {Message}", arg.Message);
                    break;

                default:
                    _logger.LogWarning("Incorrect LogMessage.Severity - {Severity}, {Message}", arg.Severity, arg.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        private Task BotMessageReceivedAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage { Source: MessageSource.User } message)
                return Task.CompletedTask;
            if (message.Channel is IPrivateChannel)
                return Task.CompletedTask;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _discordSocketClient?.Dispose();
            base.Dispose();
        }

        public async Task Handle(PlayerJoinedNotification notification, CancellationToken cancellationToken)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `EVENT  : Player {notification.Player.Name} joined the server!`");
        }

        public async Task Handle(PlayerLeftNotification notification, CancellationToken cancellationToken)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `EVENT  : Player {notification.Name} left the server!`");
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken cancellationToken)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `MESSAGE: <{notification.Player.Name}> {notification.Message}`");
        }

        public async Task Handle(ServerMessageNotification notification, CancellationToken cancellationToken)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `SERVER : {notification.Message}`");
        }

        public async Task Handle(PlayerTriggeredEventNotification notification, CancellationToken cancellationToken)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `EVENT  : The player {notification.Player.Name} {notification.EventMessage}`");
        }
    }
}