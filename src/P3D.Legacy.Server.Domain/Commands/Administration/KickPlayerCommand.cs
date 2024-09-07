namespace P3D.Legacy.Server.Domain.Commands.Administration;

public sealed record KickPlayerCommand(IPlayer Player, string Reason) : ICommand;