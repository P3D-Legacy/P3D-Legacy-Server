using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Domain.Commands.Player;

public sealed record PlayerMutedPlayerCommand(PlayerId Id, PlayerId IdToMute) : ICommand;