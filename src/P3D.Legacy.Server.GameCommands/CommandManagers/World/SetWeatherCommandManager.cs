using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.World;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.World;

internal class SetWeatherCommandManager : CommandManager
{
    public override string Name => "setweather";
    public override string Description => "Set World Weather.";
    public override IEnumerable<string> Aliases => new[] { "sw" };
    public override PermissionTypes Permissions => PermissionTypes.ModeratorOrHigher;

    public SetWeatherCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
    {
        if (arguments.Length == 1)
        {
            if (Enum.TryParse(arguments[0], true, out WorldWeather weather))
            {
                await CommandDispatcher.DispatchAsync(new ChangeWorldWeatherCommand(weather), ct);
                await SendMessageAsync(player, $"Set Weather to {weather}!", ct);
            }
            else
            {
                await SendMessageAsync(player, $"Weather '{arguments[0]}' not found!", ct);
            }
        }
        else
        {
            await SendMessageAsync(player, "Invalid arguments given.", ct);
        }
    }

    public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
    {
        await SendMessageAsync(player, $"Correct usage is /{alias} <weather>", ct);
    }
}