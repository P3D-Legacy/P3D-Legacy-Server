using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Application.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Client
{
    public class UnbanCommandManager : CommandManager
    {
        public override string Name => "unban";
        public override string Description => "Unban a Player.";
        public override IEnumerable<string> Aliases => new[] { "ub" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        public UnbanCommandManager(IMediator mediator, IPlayerContainerReader playerContainer) : base(mediator, playerContainer) { }

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

                await Mediator.Publish(new UnbanPlayerCommand(cClient.GameJoltId), ct);
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