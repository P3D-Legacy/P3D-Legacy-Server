using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.CQERS.Commands;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command
{
    [SuppressMessage("Performance", "CA1812")]
    internal class CommandPerformanceBehaviour<TCommand> : ICommandBehavior<TCommand> where TCommand : ICommand
    {
        private static readonly Action<ILogger, string, long, TCommand, Exception?> Command = LoggerMessage.Define<string, long, TCommand>(
            LogLevel.Warning, default, "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Command}");

        private readonly ILogger<TCommand> _logger;
        private readonly Stopwatch _timer = new();

        [SuppressMessage("CodeQuality", "IDE0079")]
        [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
        public CommandPerformanceBehaviour(ILogger<TCommand> logger)
        {
            _logger = logger;
        }

        public async Task<CommandResult> HandleAsync(TCommand command, CommandHandlerDelegate next, CancellationToken ct)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var commandName = typeof(TCommand).Name;

                Command(_logger, commandName, elapsedMilliseconds, command, null);
            }

            return response;
        }
    }
}