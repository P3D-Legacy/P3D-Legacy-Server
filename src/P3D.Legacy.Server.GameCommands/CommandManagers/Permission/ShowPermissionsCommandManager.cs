using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Permission
{
    public class ShowPermissionsCommandManager : CommandManager
    {
        public override string Name => "showperm";
        public override string Description => "Show available Client permissions.";
        public override PermissionFlags Permissions => PermissionFlags.AdministratorOrHigher;

        public ShowPermissionsCommandManager(IMediator mediator, IPlayerContainerReader playerContainer) : base(mediator, playerContainer) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
                await SendMessageAsync(client, string.Join(",", Enum.GetNames(typeof(PermissionFlags))), ct);
            else if (arguments.Length == 2)
            {
                var clientName = arguments[0];

                var cClient = await GetClientAsync(clientName, ct);
                if (cClient == null)
                {
                    await SendMessageAsync(client, $"Player {clientName} not found.", ct);
                    return;
                }

                await SendMessageAsync(client, $"Player {clientName} permissions are {client.Permissions.ToString()}.", ct);
            }
            else
                await SendMessageAsync(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessageAsync(client, $"Correct usage is /{alias} [PlayerName]", ct);
        }
    }
}