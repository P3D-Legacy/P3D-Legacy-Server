using BetterHostedServices;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Common.PlayerEvents;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.DiscordBot.Options;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.DiscordBot.BackgroundServices
{
    internal sealed partial class DiscordPassthroughService : IHostedService, IDisposable,
        IEventHandler<PlayerJoinedEvent>,
        IEventHandler<PlayerLeftEvent>,
        IEventHandler<PlayerSentGlobalMessageEvent>,
        IEventHandler<ServerMessageEvent>,
        IEventHandler<PlayerTriggeredEventEvent>
    {
        private Task? _executingTask;

        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient? _discordSocketClient;
        private readonly DiscordOptions _options;
        private readonly CancellationTokenSource _stoppingCts = new();
        private readonly IApplicationEnder _applicationEnder;

        public DiscordPassthroughService(
            ILogger<DiscordPassthroughService> logger,
            TracerProvider traceProvider,
            DiscordSocketClient discordSocketClient,
            IServiceScopeFactory scopeFactory,
            IOptions<DiscordOptions> options,
            IApplicationEnder applicationEnder)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.DiscordBot");
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _discordSocketClient = options.Value.PasstroughChannelId != 0
                ? discordSocketClient ?? throw new ArgumentNullException(nameof(discordSocketClient))
                : null;
            _applicationEnder = applicationEnder;
        }

        private void OnError(Exception exceptionFromExecuteAsync)
        {
            Console.Error.WriteLine($"Error happened while executing CriticalBackgroundTask {typeof(DiscordPassthroughService).FullName}. Shutting down.");
            Console.Error.WriteLine(exceptionFromExecuteAsync.ToString());
            _applicationEnder.ShutDownApplication();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Store the task we're executing
            _executingTask = Task.Factory.StartNew(() => ExecuteAsync(_stoppingCts.Token), _stoppingCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            // Add an error handler here that shuts down the application
            // Do not save it to _executingTask - as that means it will hang on shutting down
            // until the grace period is over.
            _ = HandleExecutingTaskAsync(_executingTask);

            // Otherwise it's running
            return Task.CompletedTask;
        }

        private async Task HandleExecutingTaskAsync(Task executingTask)
        {
            try
            {
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                await executingTask;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
            }
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var span = _tracer.StartActiveSpan("Discord Bot", SpanKind.Internal);

            void OnCancellation(object? _, CancellationToken ct)
            {
                ct.ThrowIfCancellationRequested();
                _discordSocketClient.MessageReceived -= BotMessageReceivedAsync;
                _discordSocketClient.Log -= BotLogAsync;
                _discordSocketClient.StopAsync();
                _logger.LogWarning("Stopped Discord Bot");
            }

            if (_discordSocketClient is null)
                return;

            using var scope = _scopeFactory.CreateScope();

            if (_discordSocketClient.ConnectionState is not ConnectionState.Disconnecting and not ConnectionState.Disconnected)
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

            stoppingToken.Register(OnCancellation, null);
        }

        [LoggerMessage(Level = LogLevel.Critical, Message = "Critical log entry: {Message}")]
        private partial void LogCritical(string message, Exception exception);
        [LoggerMessage(Level = LogLevel.Error, Message = "Error log entry: {Message}")]
        private partial void LogError(string message, Exception exception);
        [LoggerMessage(Level = LogLevel.Warning, Message = "Warning log entry: {Message}")]
        private partial void LogWarning(string message, Exception exception);
        [LoggerMessage(Level = LogLevel.Information, Message = "Information log entry: {Message}")]
        private partial void LogInfo(string message, Exception exception);
        [LoggerMessage(Level = LogLevel.Trace, Message = "Trace log entry: {Message}")]
        private partial void LogVerbose(string message, Exception exception);
        [LoggerMessage(Level = LogLevel.Debug, Message = "Debug log entry: {Message}")]
        private partial void LogDebug(string message, Exception exception);
        [LoggerMessage(Level = LogLevel.Warning, Message = "Incorrect LogMessage.Severity - {Severity}, {Message}")]
        private partial void LogUnknown(LogSeverity severity, string message, Exception exception);


        private Task BotLogAsync(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    LogCritical(arg.Message, arg.Exception);
                    break;
                case LogSeverity.Error:
                    LogError(arg.Message, arg.Exception);
                    break;
                case LogSeverity.Warning:
                    LogWarning(arg.Message, arg.Exception);
                    break;
                case LogSeverity.Info:
                    LogInfo(arg.Message, arg.Exception);
                    break;
                case LogSeverity.Verbose:
                    LogVerbose(arg.Message, arg.Exception);
                    break;
                case LogSeverity.Debug:
                    LogDebug(arg.Message, arg.Exception);
                    break;

                default:
                    LogUnknown(arg.Severity, arg.Message, arg.Exception);
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

        public void Dispose()
        {
            _stoppingCts.Cancel();
            _stoppingCts.Dispose();
        }

        public async Task HandleAsync(IReceiveContext<PlayerJoinedEvent> context, CancellationToken ct)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `EVENT  : Player {context.Message.Player.Name} joined the server!`");
        }

        public async Task HandleAsync(IReceiveContext<PlayerLeftEvent> context, CancellationToken ct)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `EVENT  : Player {context.Message.Name} left the server!`");
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentGlobalMessageEvent> context, CancellationToken ct)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `MESSAGE: <{context.Message.Player.Name}> {context.Message.Message}`");
        }

        public async Task HandleAsync(IReceiveContext<ServerMessageEvent> context, CancellationToken ct)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `SERVER : {context.Message.Message}`");
        }

        public async Task HandleAsync(IReceiveContext<PlayerTriggeredEventEvent> context, CancellationToken ct)
        {
            if (_discordSocketClient?.GetChannel(_options.PasstroughChannelId) as ISocketMessageChannel is { } channel)
                await channel.SendMessageAsync($"> `EVENT  : The player {context.Message.Player.Name} {PlayerEventParser.AsText(context.Message.Event)}`");
        }
    }
}