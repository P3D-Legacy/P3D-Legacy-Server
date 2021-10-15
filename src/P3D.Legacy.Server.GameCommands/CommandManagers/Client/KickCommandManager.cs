using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Application.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Client
{
    public class KickCommandManager : CommandManager
    {
        public override string Name => "kick";
        public override string Description => "Kick a Player.";
        public override IEnumerable<string> Aliases => new[] { "k" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        public KickCommandManager(IMediator mediator, IPlayerContainerReader playerContainer) : base(mediator, playerContainer) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var clientName = arguments[0];
                var cClient = await GetClientAsync(clientName, ct);
                if (cClient == null)
                {
                    await SendMessageAsync(client, $"Player {clientName} not found!", ct);
                    return;
                }

                await Mediator.Publish(new KickPlayerCommand(cClient, "Kicked by a Moderator or Admin."), ct);
            }
            else if (arguments.Length > 1)
            {
                var clientName = arguments[0];
                var cClient = await GetClientAsync(clientName, ct);
                if (cClient == null)
                {
                    await SendMessageAsync(client, $"Player {clientName} not found!", ct);
                    return;
                }

                var reason = string.Join(" ", arguments.Skip(1).ToArray());
                await Mediator.Publish(new KickPlayerCommand(cClient, reason), ct);
            }
            else
                await SendMessageAsync(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessageAsync(client, $"Correct usage is /{alias} <PlayerName> [Reason]", ct);
        }
    }
}