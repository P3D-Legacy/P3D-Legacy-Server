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
    public class SetSeasonCommandManager : CommandManager
    {
        public override string Name => "setseason";
        public override string Description => "Set World Season.";
        public override IEnumerable<string> Aliases => new[] { "ss" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        public SetSeasonCommandManager(IMediator mediator, IPlayerContainerReader playerContainer) : base(mediator, playerContainer) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                if (Enum.TryParse(arguments[0], true, out WorldSeason season))
                {
                    await Mediator.Send(new ChangeWorldSeasonCommand(season), ct);
                    await SendMessageAsync(player, $"Set Season to {season}!", ct);
                }
                else
                    await SendMessageAsync(player, $"Season '{arguments[0]}' not found!", ct);
            }
            else
                await SendMessageAsync(player, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <season>", ct);
        }
    }
}