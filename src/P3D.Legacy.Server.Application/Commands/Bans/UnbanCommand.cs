using MediatR;

using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Application.Commands.Bans
{
    public record UnbanCommand(GameJoltId Id) : IRequest<CommandResult>;
}