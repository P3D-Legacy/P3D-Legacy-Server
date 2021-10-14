using MediatR;

using P3D.Legacy.Server.Abstractions;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.Client
{
    public class KickCommandManager : CommandManager
    {
        public override string Name => "kick";
        public override string Description => "Kick a Player.";
        public override IEnumerable<string> Aliases => new [] { "k" };
        public override Permissions Permissions => Permissions.ModeratorOrHigher;

        public KickCommandManager(IMediator mediator) : base(mediator) { }

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

                //ModuleManager.Kick(cClient, "Kicked by a Moderator or Admin.");
            }
            else if (arguments.Length > 1)
            {
                var clientName = arguments[0];
                var cClient = GetClient(clientName);
                if (cClient == null)
                {
                    await SendMessage(client, $"Player {clientName} not found!", ct);
                    return;
                }

                var reason = string.Join(" ", arguments.Skip(1).ToArray());
                //ModuleManager.Kick(cClient, reason);
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