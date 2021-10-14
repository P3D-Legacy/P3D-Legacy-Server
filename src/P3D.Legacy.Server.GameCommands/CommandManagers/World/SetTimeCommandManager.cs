using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.World
{
    public class SetTimeCommandManager : CommandManager
    {
        public override string Name => "settime";
        public override string Description => "Set World Time.";
        public override IEnumerable<string> Aliases => new [] { "st" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        private readonly WorldService _worldService;

        public SetTimeCommandManager(IMediator mediator, WorldService worldService) : base(mediator)
        {
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
        }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                if (TimeSpan.TryParseExact(arguments[0], "HH\\:mm\\:ss", null, out var time))
                {
                    _worldService.CurrentTime = time;
                    await SendMessageAsync(client, $"Set time to {time}!", ct);
                    await SendMessageAsync(client, "Disabled Real Time!", ct);
                }
                else
                    await SendMessageAsync(client, "Invalid time!", ct);
            }
            else
                await SendMessageAsync(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessageAsync(client, $"Correct usage is /{alias} <Time[HH:mm:ss]/Real>", ct);
        }
    }
}