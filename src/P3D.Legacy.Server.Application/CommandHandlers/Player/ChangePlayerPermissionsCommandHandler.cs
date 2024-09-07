using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.Infrastructure.Repositories.Permissions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player;

internal sealed class ChangePlayerPermissionsCommandHandler : ICommandHandler<ChangePlayerPermissionsCommand>
{
    private readonly ILogger _logger;
    private readonly IPermissionRepository _permissionManager;

    public ChangePlayerPermissionsCommandHandler(ILogger<ChangePlayerPermissionsCommandHandler> logger, IPermissionRepository permissionManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
    }

    public async Task<CommandResult> HandleAsync(ChangePlayerPermissionsCommand command, CancellationToken ct)
    {
        var (player, permissions) = command;

        var result = await _permissionManager.SetPermissionsAsync(player.Id, permissions, ct);
        if (result)
        {
            await player.AssignPermissionsAsync(permissions, ct);
        }

        return new CommandResult(result);
    }
}