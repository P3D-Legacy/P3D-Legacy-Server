using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player
{
    internal class BanCommandManager : CommandManager
    {
        public override string Name => "ban";
        public override string Description => "Ban a Player.";
        public override IEnumerable<string> Aliases => new[] { "b" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        public BanCommandManager(IMediator mediator, IPlayerContainerReader playerContainer) : base(mediator, playerContainer) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 3)
            {
                var targetName = arguments[0];
                if (await GetPlayerAsync(targetName, ct) is not { } targetPlayer)
                {
                    await SendMessageAsync(player, $"Player {targetName} not found!", ct);
                    return;
                }

                if (targetPlayer.Id == player.Id)
                {
                    await SendMessageAsync(player, "You can't ban yourself!", ct);
                    return;
                }

                if (!int.TryParse(arguments[1], out var minutes))
                {
                    await SendMessageAsync(player, "Invalid minutes given.", ct);
                    return;
                }

                var reason = arguments[2].TrimStart('"').TrimEnd('"');

                if (ulong.TryParse(reason, out var reasonId))
                    reason = string.Empty;

                await Mediator.Send(new KickPlayerCommand(targetPlayer, $"You are banned: {reason}"), ct);
                await Mediator.Send(new BanPlayerCommand(player.Id, targetPlayer.Id, targetPlayer.IPEndPoint.Address, reasonId, reason, DateTimeOffset.UtcNow.AddMinutes(minutes)), ct);
            }
            else if (arguments.Length > 3)
            {
                var targetName = arguments[0];
                if (await GetPlayerAsync(targetName, ct) is not { } targetPlayer)
                {
                    await SendMessageAsync(player, $"Player {targetName} not found!", ct);
                    return;
                }

                if (targetPlayer.Id == player.Id)
                {
                    await SendMessageAsync(player, "You can't ban yourself!", ct);
                    return;
                }

                if (!int.TryParse(arguments[1], out var minutes))
                {
                    await SendMessageAsync(player, "Invalid minutes given.", ct);
                    return;
                }

                var reason = string.Join(" ", arguments.Skip(2).ToArray());

                if (ulong.TryParse(reason, out var reasonId))
                    reason = string.Empty;

                await Mediator.Send(new KickPlayerCommand(targetPlayer, $"You are banned: {reason}"), ct);
                await Mediator.Send(new BanPlayerCommand(player.Id, targetPlayer.Id, targetPlayer.IPEndPoint.Address, reasonId, reason, DateTimeOffset.UtcNow.AddMinutes(minutes)), ct);
            }
            else
                await SendMessageAsync(player, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <playername> [<reason>]", ct);
        }
    }
}