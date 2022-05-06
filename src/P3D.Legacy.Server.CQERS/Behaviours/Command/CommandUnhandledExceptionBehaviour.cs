using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.CQERS.Commands;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command
{
    [SuppressMessage("Performance", "CA1812")]
    internal class CommandUnhandledExceptionBehaviour<TCommand> : ICommandBehavior<TCommand> where TCommand : ICommand
    {
        private static readonly Action<ILogger, string, TCommand, Exception?> Command = LoggerMessage.Define<string, TCommand>(
            LogLevel.Error, default, "Request: Unhandled Exception for Request {Name} {@Command}");

        private readonly ILogger<TCommand> _logger;

        [SuppressMessage("CodeQuality", "IDE0079")]
        [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
        public CommandUnhandledExceptionBehaviour(ILogger<TCommand> logger)
        {
            _logger = logger;
        }

        public async Task<CommandResult> HandleAsync(TCommand request, CommandHandlerDelegate next, CancellationToken ct)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TCommand).Name;

                Command(_logger, requestName, request, ex);

                throw;
            }
        }
    }
}