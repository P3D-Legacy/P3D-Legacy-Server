using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.CQERS.Commands;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

internal partial class CommandPerformanceBehaviour<TCommand> : ICommandBehavior<TCommand> where TCommand : ICommand
{
#if FALSE // TODO: https://github.com/dotnet/runtime/issues/60968
        [LoggerMessage(Level = LogLevel.Warning, Message = "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Command}")]
#else
    [LoggerMessage(Level = LogLevel.Warning, Message = "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {Command}")]
#endif
    private partial void Command(string name, long elapsedMilliseconds, TCommand command);

    private readonly ILogger<TCommand> _logger;
    private readonly Stopwatch _timer = new();

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

            Command(commandName, elapsedMilliseconds, command);
        }

        return response;
    }
}