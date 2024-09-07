namespace P3D.Legacy.Server.Domain.Commands.Player;

public sealed record PlayerInitializingCommand(IPlayer Player) : ICommand;