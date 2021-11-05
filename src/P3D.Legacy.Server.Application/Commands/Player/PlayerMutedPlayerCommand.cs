using MediatR;

using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public record PlayerMutedPlayerCommand(PlayerId Id, PlayerId IdToMute) : IRequest<CommandResult>;
}