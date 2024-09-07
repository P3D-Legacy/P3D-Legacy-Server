using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Domain.Commands.Player;

public sealed record PlayerUnmutedPlayerCommand(PlayerId Id, PlayerId IdToUnmute) : ICommand;