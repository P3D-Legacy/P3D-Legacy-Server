using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal class UnmuteCommandManager : CommandManager
    {
        public override string Name => "unmute";
        public override string Description => "Command is disabled";
        public override IEnumerable<string> Aliases => new[] { "um" };
        public override PermissionTypes Permissions => PermissionTypes.UserOrHigher;

        public UnmuteCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var targetName = arguments[0];
                if (await GetPlayerAsync(targetName, ct) is not { } targetPlayer)
                {
                    await SendMessageAsync(player, $"Player {targetName} not found!", ct);
                    return;
                }

                if (targetPlayer.Id == player.Id)
                {
                    await SendMessageAsync(player, "You can't unmute yourself!", ct);
                    return;
                }

                var result = await CommandDispatcher.DispatchAsync(new PlayerUnmutedPlayerCommand(player.Id, targetPlayer.Id), ct);
                if (!result.IsSuccess)
                    await SendMessageAsync(player, $"Failed to unmute player {targetName}!", ct);
            }
            else
            {
                await SendMessageAsync(player, "Invalid arguments given.", ct);
            }
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <playername>", ct);
        }
    }
}