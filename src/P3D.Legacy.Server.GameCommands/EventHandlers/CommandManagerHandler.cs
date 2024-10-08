﻿using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Events.Player;
using P3D.Legacy.Server.Domain.Extensions;
using P3D.Legacy.Server.GameCommands.CommandManagers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.EventHandlers;

internal sealed partial class CommandManagerHandler :
    IEventHandler<PlayerSentCommandEvent>
{
    [LoggerMessage(Level = LogLevel.Information, Message = "{PlayerName}: /{CommandAlias} {CommandArgs}")]
    private partial void Command(string playerName, string commandAlias, string commandArgs);

    [GeneratedRegex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Regex();

    private readonly ILogger _logger;
    private readonly Tracer _tracer;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IReadOnlyList<CommandManager> _commandManagers;

    public CommandManagerHandler(ILogger<CommandManagerHandler> logger, TracerProvider traceProvider, IEventDispatcher eventDispatcher, IEnumerable<CommandManager> commandManagers)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _commandManagers = commandManagers.ToList();
        _tracer = traceProvider.GetTracer("P3D.Legacy.Server.GameCommands");
    }

    private async Task SendMessageAsync(IPlayer player, string message, CancellationToken ct)
    {
        await _eventDispatcher.DispatchAsync(new MessageToPlayerEvent(IPlayer.Server, player, message), ct);
    }

    private async Task HandleCommandAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
    {
        using var span = _tracer.StartActiveSpan("CommandManagerHandler HandleCommand");

        var command = FindByName(alias) ?? FindByAlias(alias);
        if (command is null)
        {
            await SendMessageAsync(player, $@"Invalid command ""{alias}"".", ct);
            return;
        }

        if (command.LogCommand && (player.Permissions & PermissionTypes.UnVerified) == PermissionTypes.None)
        {
            Command(player.Name, alias, string.Join(' ', arguments));
        }

        if (command.Permissions == PermissionTypes.None)
        {
            await SendMessageAsync(player, "Command is not found!", ct);
            //await SendMessageAsync(player, @"Command is disabled!", ct);
            return;
        }

        if ((player.Permissions & command.Permissions) == PermissionTypes.None)
        {
            await SendMessageAsync(player, "Command is not found!", ct);
            //await SendMessageAsync(player, @"You have not the permission to use this command!", ct);
            return;
        }

        using var commandSpan = _tracer.StartActiveSpan($"{command.GetType().Name} Handle");
        await command.HandleAsync(player, alias, arguments, ct);
    }

    public CommandManager? FindByName(string name) => _commandManagers
        .Where(static x => x.Permissions != PermissionTypes.None)
        .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    public CommandManager? FindByAlias(string alias) => _commandManagers
        .Where(static x => x.Permissions != PermissionTypes.None)
        .FirstOrDefault(x => x.Aliases.Contains(alias, StringComparer.OrdinalIgnoreCase));

    public IReadOnlyList<CommandManager> GetCommands() => _commandManagers;

    public async Task HandleAsync(IReceiveContext<PlayerSentCommandEvent> context, CancellationToken ct)
    {
        using var span = _tracer.StartActiveSpan("CommandManagerHandler Handle");

        var (player, message) = context.Message;

        var commandWithoutSlash = message.TrimStart('/');

        var messageArray = Regex()
            .Split(commandWithoutSlash)
            .Select(static str => str.TrimStart('"').TrimEnd('"'))
            .ToArray();

        if (messageArray.Length == 0)
        {
            await SendMessageAsync(player, "Invalid command!", ct);
            return;
        }

        var alias = messageArray[0];
        var trimmedMessageArray = messageArray.Skip(1).ToArray();
        await HandleCommandAsync(player, alias, trimmedMessageArray, ct);
    }
}