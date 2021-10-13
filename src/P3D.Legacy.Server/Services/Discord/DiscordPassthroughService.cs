using BetterHostedServices;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Models;
using P3D.Legacy.Server.Models.Options;
using P3D.Legacy.Server.Notifications;
using P3D.Legacy.Server.Queries.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Discord
{
    public sealed class DiscordPassthroughService : CriticalBackgroundService,
        INotificationHandler<PlayerSentGlobalMessageNotification>
    {
        /*
        private record DiscordBotPlayer : IPlayer
        {
            public Origin Id { get; } = Origin.Server;
            public string Name { get; } = "Discord Bot";
            public ulong GameJoltId { get; } = 0;
            public string ConnectionId { get; } = "DISCORDBOT";
            public Permissions Permissions { get; } = Permissions.Administrator;

            public Task AssignIdAsync(long id, CancellationToken ct) => throw new NotSupportedException();
        }
        */

        //private static readonly IPlayer DiscordPlayer = new();

        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly DiscordOptions _options;

        public DiscordPassthroughService(
            ILogger<DiscordPassthroughService> logger,
            DiscordSocketClient discordSocketClient,
            IServiceScopeFactory scopeFactory,
            IOptions<DiscordOptions> options,
            IMediator mediator,
            IApplicationEnder applicationEnder) : base(applicationEnder)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _mediator = mediator;
            _discordSocketClient = discordSocketClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            async void OnCancellation(object? _, CancellationToken ct)
            {
                _discordSocketClient.MessageReceived -= BotMessageReceivedAsync;
                _discordSocketClient.Log -= BotLogAsync;
                await _discordSocketClient.StopAsync();
                _logger.LogWarning("Stopped Discord Bot");
            }

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

        private async Task BotMessageReceivedAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage { Source: MessageSource.User } message) return;
            if (message.Channel is IPrivateChannel) return;

            var context = new SocketCommandContext(_discordSocketClient, message);

            //var result = await _mediator.Send(new RawTextCommand(DiscordPlayer, message.Content));
            //if (result.Success)
            //    await context.Message.AddReactionAsync(new Emoji("⁉️"));
            //else
            //    await context.Message.AddReactionAsync(new Emoji("❓"));
        }

        public override void Dispose()
        {
            _discordSocketClient.Dispose();
            base.Dispose();
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken cancellationToken)
        {
            if (_discordSocketClient.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"<{notification.Name}> {notification.Message}");
        }
    }
}
