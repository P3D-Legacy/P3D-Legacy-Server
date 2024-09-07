namespace P3D.Legacy.Server.Domain.Commands.Player;

public sealed record PlayerReadyCommand(IPlayer Player) : ICommand;