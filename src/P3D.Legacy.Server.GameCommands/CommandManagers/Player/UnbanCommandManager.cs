using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Administration;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal class UnbanCommandManager : CommandManager
    {
        public override string Name => "unban";
        public override string Description => "Unban a Player.";
        public override IEnumerable<string> Aliases => new[] { "ub" };
        public override PermissionTypes Permissions => PermissionTypes.ModeratorOrHigher;

        public UnbanCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var targetName = arguments[0];
                var playerId = PlayerId.Parse(targetName);
                if (playerId == PlayerId.None)
                {
                    await SendMessageAsync(player, $"Please input the player id as \"GameJolt:GAMEJOLTID\" for GameJolt and \"Name:NAME\" for offline players", ct);
                    return;
                }

                var result = await CommandDispatcher.DispatchAsync(new UnbanPlayerCommand(playerId), ct);
                if (!result.IsSuccess)
                    await SendMessageAsync(player, $"Failed to unban player {targetName}!", ct);
            }
            else
            {
                await SendMessageAsync(player, "Invalid arguments given.", ct);
            }
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <playername> [<reason>]", ct);
        }
    }
}