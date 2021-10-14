using MediatR;

using P3D.Legacy.Server.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.Permission
{
    public class ShowPermissionsCommandManager : CommandManager
    {
        public override string Name => "showperm";
        public override string Description => "Show available Client permissions.";
        public override Permissions Permissions => Permissions.AdministratorOrHigher;

        public ShowPermissionsCommandManager(IMediator mediator) : base(mediator) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
                await SendMessage(client, string.Join(",", Enum.GetNames(typeof(Permissions))), ct);
            else if (arguments.Length == 2)
            {
                var clientName = arguments[0];

                var cClient = GetClient(clientName);
                if (cClient == null)
                {
                    await SendMessage(client, $"Player {clientName} not found.", ct);
                    return;
                }

                await SendMessage(client, $"Player {clientName} permissions are {client.Permissions.ToString()}.", ct);
            }
            else
                await SendMessage(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessage(client, $"Correct usage is /{alias} [PlayerName]", ct);
        }
    }
}