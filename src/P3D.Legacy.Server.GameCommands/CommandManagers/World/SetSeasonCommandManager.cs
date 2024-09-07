using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Commands.World;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.World;

internal class SetSeasonCommandManager : CommandManager
{
    public override string Name => "setseason";
    public override string Description => "Set World Season.";
    public override IEnumerable<string> Aliases => new[] { "ss" };
    public override PermissionTypes Permissions => PermissionTypes.ModeratorOrHigher;

    public SetSeasonCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
    {
        if (arguments.Length == 1)
        {
            if (Enum.TryParse(arguments[0], true, out WorldSeason season))
            {
                await CommandDispatcher.DispatchAsync(new ChangeWorldSeasonCommand(season), ct);
                await SendMessageAsync(player, $"Set Season to {season}!", ct);
            }
            else
            {
                await SendMessageAsync(player, $"Season '{arguments[0]}' not found!", ct);
            }
        }
        else
        {
            await SendMessageAsync(player, "Invalid arguments given.", ct);
        }
    }

    public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
    {
        await SendMessageAsync(player, $"Correct usage is /{alias} <season>", ct);
    }
}