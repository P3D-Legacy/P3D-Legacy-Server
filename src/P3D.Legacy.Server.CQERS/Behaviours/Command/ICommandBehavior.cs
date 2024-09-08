using P3D.Legacy.Server.Domain.Commands;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

public interface ICommandBehavior<in TCommand> where TCommand : ICommand
{
    int Order { get; }

    Task<CommandResult> HandleAsync(TCommand command, CommandHandlerDelegate next, CancellationToken ct);
}