using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player
{
    internal class MuteCommandManager : CommandManager
    {
        public override string Name => "mute";
        public override string Description => "Command is disabled";
        public override IEnumerable<string> Aliases => new[] { "mm" };
        public override PermissionTypes Permissions => PermissionTypes.UserOrHigher;

        public MuteCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

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
                    await SendMessageAsync(player, "You can't mute yourself!", ct);
                    return;
                }

                var result = await CommandDispatcher.DispatchAsync(new PlayerMutedPlayerCommand(player.Id, targetPlayer.Id), ct);
                if (!result.IsSuccess)
                    await SendMessageAsync(player, $"Failed to mute player {targetName}!", ct);
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