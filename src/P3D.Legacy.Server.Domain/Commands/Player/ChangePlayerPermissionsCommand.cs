namespace P3D.Legacy.Server.Domain.Commands.Player;

public sealed record ChangePlayerPermissionsCommand(IPlayer Player, PermissionTypes Permissions) : ICommand;