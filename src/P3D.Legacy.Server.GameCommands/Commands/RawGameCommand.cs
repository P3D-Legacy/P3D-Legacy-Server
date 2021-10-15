using MediatR;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands;

namespace P3D.Legacy.Server.GameCommands.Commands
{
    public record RawGameCommand(IPlayer Player, string Command) : IRequest<CommandResult>;
}