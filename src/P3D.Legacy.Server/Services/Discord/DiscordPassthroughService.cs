using BetterHostedServices;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Models.Options;
using P3D.Legacy.Server.Notifications;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Discord
{
    public sealed class DiscordPassthroughService : CriticalBackgroundService,
        INotificationHandler<PlayerSentGlobalMessageNotification>
    {
        private readonly DiscordSocketClient _discordSocketClient;
        //private readonly CommandService _commands;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private readonly DiscordOptions _options;

        public DiscordPassthroughService(
            ILogger<DiscordPassthroughService> logger,
            DiscordSocketClient discordSocketClient,
            //CommandService commands,
            IServiceScopeFactory scopeFactory,
            IOptions<DiscordOptions> options,
            IApplicationEnder applicationEnder) : base(applicationEnder)
        {
            _logger = logger;
            //_commands = commands;
            _scopeFactory = scopeFactory;
            _options = options.Value;
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
           // await _commands.AddModulesAsync(typeof(DiscordCommands).Assembly, scope.ServiceProvider);

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
            /*
            if (arg is not SocketUserMessage { Source: MessageSource.User } message) return;
            if (message.Channel is IPrivateChannel) return;

            var argPos = 0;
            if (message.HasStringPrefix("!nmm ", ref argPos) || message.HasMentionPrefix(_discordSocketClient.CurrentUser, ref argPos))
            {
                using var scope = _scopeFactory.CreateScope();
                var context = new SocketCommandContext(_discordSocketClient, message);
                var result = await _commands.ExecuteAsync(context, argPos, scope.ServiceProvider);

                if (result.Error.HasValue && result.Error.Value != CommandError.UnknownCommand)
                {
                    _logger.LogError("Error! {Result}", result.ToString());
                    await context.Message.AddReactionAsync(new Emoji("⁉️"));
                }
                else if (result.Error is CommandError.UnknownCommand)
                {
                    await context.Message.AddReactionAsync(new Emoji("❓"));
                }
            }
            */
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
