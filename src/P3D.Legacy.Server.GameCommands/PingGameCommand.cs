using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands;

namespace P3D.Legacy.Server.GameCommands
{
    public record PingGameCommand(IPlayer Player) : IRequest<CommandResult>;
}