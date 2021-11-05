using MediatR;

using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public record PlayerUnmutedPlayerCommand(PlayerId Id, PlayerId IdToUnmute) : IRequest<CommandResult>;
}