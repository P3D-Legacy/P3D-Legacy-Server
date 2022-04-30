using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions.Commands;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public sealed record PlayerMutedPlayerCommand(PlayerId Id, PlayerId IdToMute) : ICommand;
}