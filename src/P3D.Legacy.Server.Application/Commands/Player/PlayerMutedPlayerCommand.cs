using P3D.Legacy.Common;
using P3D.Legacy.Server.CQERS.Commands;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public sealed record PlayerMutedPlayerCommand(PlayerId Id, PlayerId IdToMute) : ICommand;
}