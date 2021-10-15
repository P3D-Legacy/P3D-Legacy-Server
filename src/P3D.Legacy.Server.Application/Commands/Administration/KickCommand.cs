using MediatR;

using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Application.Commands.Administration
{
    public record KickCommand(IPlayer Player, string Reason) : IRequest<CommandResult>;
}