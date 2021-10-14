using MediatR;

using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.World
{
    public class SetWeatherCommandManager : CommandManager
    {
        public override string Name => "setweather";
        public override string Description => "Set World Weather.";
        public override IEnumerable<string> Aliases => new [] { "sw" };
        public override Permissions Permissions => Permissions.ModeratorOrHigher;

        private readonly WorldService _worldService;

        public SetWeatherCommandManager(IMediator mediator, WorldService worldService) : base(mediator)
        {
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
        }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                if (Enum.TryParse(arguments[0], true, out WorldWeather weather))
                {
                    _worldService.Weather = weather;
                    await SendMessage(client, $"Set Weather to {weather}!", ct);
                }
                else
                    await SendMessage(client, $"Weather '{weather}' not found!", ct);
            }
            else
                await SendMessage(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessage(client, $"Correct usage is /{alias} <Weather>", ct);
        }
    }
}