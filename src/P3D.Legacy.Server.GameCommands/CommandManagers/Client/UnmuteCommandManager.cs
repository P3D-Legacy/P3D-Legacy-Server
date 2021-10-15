/*
using MediatR;

using P3D.Legacy.Server.Abstractions;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.Client
{
    public class UnmuteCommandManager : CommandManager
    {
        public override string Name => "unmute";
        public override string Description => "Command is disabled";
        public override IEnumerable<string> Aliases => new [] { "um" };
        public override Permissions Permissions => Permissions.UserOrHigher;

        public UnmuteCommandManager(IMediator mediator) : base(mediator) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            await SendMessageAsync(client, "Command not implemented.", ct);
            return;

            if (arguments.Length == 1)
            {
                var clientName = arguments[0];
                var cClient = GetClient(clientName);
                if (cClient == null)
                {
                    await SendMessageAsync(client, $"Player {clientName} not found!", ct);
                    return;
                }

            }
            else
                await SendMessageAsync(client, "Invalid arguments given.", ct);

            if (!MutedPlayers.ContainsKey(id))
                return MuteStatus.IsNotMuted;

            var muteID = Server.GetClientID(muteName);
            if (id == muteID)
                return MuteStatus.MutedYourself;

            if (muteID != -1)
            {
                MutedPlayers[id].Remove(muteID);
                return MuteStatus.Completed;
            }

            return MuteStatus.ClientNotFound;
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessageAsync(client, $"Correct usage is /{alias} <PlayerName>", ct);
        }
    }
}
*/