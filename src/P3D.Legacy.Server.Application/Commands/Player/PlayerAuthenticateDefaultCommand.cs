using MediatR;

using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public sealed record PlayerAuthenticateDefaultCommand(IPlayer Player, string Password) : IRequest<CommandResult>;
}