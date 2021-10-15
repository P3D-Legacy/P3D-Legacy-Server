using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Client
{
    public class UnmuteCommandManager : CommandManager
    {
        public override string Name => "unmute";
        public override string Description => "Command is disabled";
        public override IEnumerable<string> Aliases => new [] { "um" };
        public override PermissionFlags Permissions => PermissionFlags.UserOrHigher;

        public UnmuteCommandManager(IMediator mediator, IPlayerContainerReader playerContainer) : base(mediator, playerContainer) { }

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

                await Mediator.Send(new PlayerMutedPlayerCommand(client, cClient), ct);
            }
            else
                await SendMessageAsync(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessageAsync(client, $"Correct usage is /{alias} <PlayerName>", ct);
        }
    }
}