﻿using Microsoft.Extensions.Logging;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

internal partial class CommandLoggingBehaviour<TCommand> : ICommandPreProcessor<TCommand> where TCommand : notnull
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Command: {Name} {@Command}")]
    private partial void Command(string name, TCommand command);

    private readonly ILogger<TCommand> _logger;

    [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
    public CommandLoggingBehaviour(ILogger<TCommand> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(TCommand command, CancellationToken ct)
    {
        var commandName = typeof(TCommand).Name;

        Command(commandName, command);

        return Task.CompletedTask;
    }
}