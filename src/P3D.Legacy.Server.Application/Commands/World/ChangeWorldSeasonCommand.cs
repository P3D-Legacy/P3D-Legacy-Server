using MediatR;

using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Server.Application.Commands.World
{
    public record ChangeWorldSeasonCommand(WorldSeason Season) : IRequest<CommandResult>;
}