using MediatR;

using P3D.Legacy.Server.Models;

namespace P3D.Legacy.Server.Commands
{
    public record PlayerReadyCommand(IPlayer Player) : IRequest;
}