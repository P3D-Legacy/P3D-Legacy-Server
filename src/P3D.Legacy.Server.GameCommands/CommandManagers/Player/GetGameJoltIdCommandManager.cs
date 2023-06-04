using P3D.Legacy.Server.Abstractions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal class GetGameJoltIdCommandManager : CommandManager
    {
        public override string Name => "getgamejolt";
        public override string Description => "Returns the player's GameJolt Id";
        public override IEnumerable<string> Aliases => new[] { "ggj" };
        public override PermissionTypes Permissions => PermissionTypes.ModeratorOrHigher;

        public GetGameJoltIdCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

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

                await SendMessageAsync(player, $"GameJolt Id: {targetPlayer.Id.GameJoltIdOrNone}", ct);
            }
            else
                await SendMessageAsync(player, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <playername>", ct);
        }
    }
}