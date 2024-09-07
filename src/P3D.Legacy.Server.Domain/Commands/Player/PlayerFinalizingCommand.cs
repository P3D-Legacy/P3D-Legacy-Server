namespace P3D.Legacy.Server.Domain.Commands.Player;

public sealed record PlayerFinalizingCommand(IPlayer Player) : ICommand;