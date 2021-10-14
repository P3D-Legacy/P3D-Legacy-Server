using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Bans;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.Client
{
    public class UnbanCommandManager : CommandManager
    {
        public override string Name => "unban";
        public override string Description => "Unban a Player.";
        public override IEnumerable<string> Aliases => new[] { "ub" };
        public override Permissions Permissions => Permissions.ModeratorOrHigher;

        public UnbanCommandManager(IMediator mediator) : base(mediator) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var clientName = arguments[0];
                var cClient = GetClient(clientName);
                if (cClient == null)
                {
                    await SendMessage(client, $"Player {clientName} not found!", ct);
                    return;
                }

                await Mediator.Publish(new UnbanCommand(cClient.GameJoltId), ct);
            }
            else
                await SendMessage(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessage(client, $"Correct usage is /{alias} <PlayerName> [Reason]", ct);
        }
    }
}