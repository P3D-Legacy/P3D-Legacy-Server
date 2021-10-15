using MediatR;

using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public record PlayerInitializingCommand(IPlayer Player) : IRequest;
}