using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Permission
{
    internal class SetPermissionCommand : CommandManager
    {
        public override string Name => "setperm";
        public override string Description => "Change Client permission.";
        public override IEnumerable<string> Aliases => new[] { "sperm", "sp" };
        public override PermissionFlags Permissions => PermissionFlags.AdministratorOrHigher;

        public SetPermissionCommand(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length >= 2)
            {
                var permissions = arguments.Skip(1).Where(arg => arg != "," && arg != "|").ToArray();

                var targetName = arguments[0];
                if (await GetPlayerAsync(targetName, ct) is not { } targetPlayer)
                {
                    await SendMessageAsync(player, $"Player {targetName} not found.", ct);
                    return;
                }

                var permissionFlags = await GetPermissionsAsync(player, permissions, ct).AggregateAsync(PermissionFlags.None, (current, flag) => current | flag, ct);

                var result = await Mediator.Send(new ChangePlayerPermissionsCommand(targetPlayer, permissionFlags), ct);
                if (result.Success)
                    await SendMessageAsync(player, $"Changed {targetName} permissions!", ct);
                else
                    await SendMessageAsync(player, $"Failed to change {targetName} permissions!", ct);
            }
            else
                await SendMessageAsync(player, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <PlayerName> <Permission>", ct);
        }

        private async IAsyncEnumerable<PermissionFlags> GetPermissionsAsync(IPlayer player, IEnumerable<string> permissions, [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (var permission in permissions)
            {
                if (Enum.TryParse(permission, true, out PermissionFlags flag))
                    yield return flag;
                else
                    await SendMessageAsync(player, $"Permission {permission} not found.", ct);
            }
        }
    }
}