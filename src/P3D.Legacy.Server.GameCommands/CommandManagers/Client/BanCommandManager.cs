using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Administration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Client
{
    public class BanCommandManager : CommandManager
    {
        public override string Name => "ban";
        public override string Description => "Ban a Player.";
        public override IEnumerable<string> Aliases => new[] { "b" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        public BanCommandManager(IMediator mediator) : base(mediator) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 3)
            {
                var clientName = arguments[0];
                var cClient = GetClient(clientName);
                if (cClient == null)
                {
                    await SendMessageAsync(client, $"Player {clientName} not found!", ct);
                    return;
                }

                if (!int.TryParse(arguments[1], out var minutes))
                {
                    await SendMessageAsync(client, "Invalid minutes given.", ct);
                    return;
                }

                var reason = arguments[2].TrimStart('"').TrimEnd('"');
                await Mediator.Publish(new BanCommand(cClient.GameJoltId, cClient.Name, cClient.IPAddress, reason, DateTimeOffset.UtcNow.AddMinutes(minutes)), ct);
            }
            else if (arguments.Length > 3)
            {
                var clientName = arguments[0];
                var cClient = GetClient(clientName);
                if (cClient == null)
                {
                    await SendMessageAsync(client, $"Player {clientName} not found!", ct);
                    return;
                }

                if (!int.TryParse(arguments[1], out var minutes))
                {
                    await SendMessageAsync(client, "Invalid minutes given.", ct);
                    return;
                }

                var reason = string.Join(" ", arguments.Skip(2).ToArray());
                await Mediator.Publish(new BanCommand(cClient.GameJoltId, cClient.Name, cClient.IPAddress, reason, DateTimeOffset.UtcNow.AddMinutes(minutes)), ct);
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