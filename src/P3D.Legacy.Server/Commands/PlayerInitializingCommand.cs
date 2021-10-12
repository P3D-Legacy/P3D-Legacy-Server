using MediatR;

using P3D.Legacy.Server.Models;

namespace P3D.Legacy.Server.Commands
{
    public record PlayerInitializingCommand(IPlayer Player) : IRequest;
}