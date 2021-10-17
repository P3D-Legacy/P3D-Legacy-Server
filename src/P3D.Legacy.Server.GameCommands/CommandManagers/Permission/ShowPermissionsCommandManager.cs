using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Administration;
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

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
                await SendMessageAsync(player, string.Join(",", Enum.GetNames(typeof(PermissionFlags))), ct);
            else if (arguments.Length == 2)
            {
                var targetName = arguments[0];
                if (await GetPlayerAsync(targetName, ct) is not { } targetPlayer)
                {
                    await SendMessageAsync(player, $"Player {targetName} not found.", ct);
                    return;
                }

                await SendMessageAsync(player, $"Player {targetName} permissions are {targetPlayer.Permissions}.", ct);
            }
            else
                await SendMessageAsync(player, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} [<playername>]", ct);
        }
    }
}