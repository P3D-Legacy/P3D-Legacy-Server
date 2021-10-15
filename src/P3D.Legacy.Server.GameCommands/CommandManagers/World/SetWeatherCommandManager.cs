using MediatR;

using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.World;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.World
{
    public class SetWeatherCommandManager : CommandManager
    {
        public override string Name => "setweather";
        public override string Description => "Set World Weather.";
        public override IEnumerable<string> Aliases => new[] { "sw" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        public SetWeatherCommandManager(IMediator mediator, IPlayerContainerReader playerContainer) : base(mediator, playerContainer) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                if (Enum.TryParse(arguments[0], true, out WorldWeather weather))
                {
                    await Mediator.Publish(new ChangeWorldWeatherCommand(weather), ct);
                    await SendMessageAsync(client, $"Set Weather to {weather}!", ct);
                }
                else
                    await SendMessageAsync(client, $"Weather '{arguments[0]}' not found!", ct);
            }
            else
                await SendMessageAsync(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessageAsync(client, $"Correct usage is /{alias} <Weather>", ct);
        }
    }
}