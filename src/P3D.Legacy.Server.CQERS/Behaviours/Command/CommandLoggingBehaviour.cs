using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command
{
    [SuppressMessage("Performance", "CA1812")]
    internal class CommandLoggingBehaviour<TCommand> : ICommandPreProcessor<TCommand> where TCommand : notnull
    {
        private static readonly Action<ILogger, string, TCommand, Exception?> Command = LoggerMessage.Define<string, TCommand>(
            LogLevel.Information, default, "Command: {Name} {@Command}");

        private readonly ILogger<TCommand> _logger;

        [SuppressMessage("CodeQuality", "IDE0079")]
        [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
        public CommandLoggingBehaviour(ILogger<TCommand> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(TCommand command, CancellationToken ct)
        {
            var commandName = typeof(TCommand).Name;

            Command(_logger, commandName, command, null);

            return Task.CompletedTask;
        }
    }
}