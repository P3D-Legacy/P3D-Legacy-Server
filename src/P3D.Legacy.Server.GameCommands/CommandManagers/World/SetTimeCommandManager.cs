using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.World;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.World;

internal class SetTimeCommandManager : CommandManager
{
    public override string Name => "settime";
    public override string Description => "Set World Time.";
    public override IEnumerable<string> Aliases => new[] { "st" };
    public override PermissionTypes Permissions => PermissionTypes.ModeratorOrHigher;


    public SetTimeCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
    {
        if (arguments.Length == 1)
        {
            if (TimeSpan.TryParseExact(arguments[0], "g", null, out var time))
            {
                await CommandDispatcher.DispatchAsync(new ChangeWorldTimeCommand(time), ct);
                await SendMessageAsync(player, $"Set time to {time}!", ct);
            }
            else
            {
                await SendMessageAsync(player, "Invalid time!", ct);
            }
        }
        else
        {
            await SendMessageAsync(player, "Invalid arguments given.", ct);
        }
    }

    public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
    {
        await SendMessageAsync(player, $"Correct usage is /{alias} <time[HH:mm:ss]>", ct);
    }
}