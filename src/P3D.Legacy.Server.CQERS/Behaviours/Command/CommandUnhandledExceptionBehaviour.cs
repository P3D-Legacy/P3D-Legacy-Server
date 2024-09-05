using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.CQERS.Commands;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

internal partial class CommandUnhandledExceptionBehaviour<TCommand> : ICommandBehavior<TCommand> where TCommand : ICommand
{
#if FALSE // TODO: https://github.com/dotnet/runtime/issues/60968
        [LoggerMessage(Level = LogLevel.Error, Message = "Request: Unhandled Exception for Request {Name} {@Command}")]
#else
    [LoggerMessage(Level = LogLevel.Error, Message = "Request: Unhandled Exception for Request {Name} {Command}")]
#endif
    private partial void Command(string name, TCommand command, Exception exception);

    private readonly ILogger<TCommand> _logger;

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

            Command(requestName, request, ex);

            throw;
        }
    }
}