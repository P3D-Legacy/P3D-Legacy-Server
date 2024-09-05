using P3D.Legacy.Server.CQERS.Commands;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

public interface ICommandBehavior<in TCommand> where TCommand : ICommand
{
    Task<CommandResult> HandleAsync(TCommand command, CommandHandlerDelegate next, CancellationToken ct);
}