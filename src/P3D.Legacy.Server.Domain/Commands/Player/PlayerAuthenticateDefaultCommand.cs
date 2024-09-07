namespace P3D.Legacy.Server.Domain.Commands.Player;

public sealed record PlayerAuthenticateDefaultCommand(IPlayer Player, string Password) : ICommand;