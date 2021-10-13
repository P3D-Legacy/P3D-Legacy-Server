using MediatR;

using P3D.Legacy.Server.Models;

namespace P3D.Legacy.Server.Commands
{
    public record CommandResult<TResult>(bool Success, TResult Result);
    public record CommandResult(bool Success) : CommandResult<Unit>(Success, Unit.Value);

    public record RawTextCommand(IPlayer Player, string Message) : IRequest<CommandResult>;
}