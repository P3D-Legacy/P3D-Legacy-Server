using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Commands;

public interface ICommandDispatcher
{
    Task<CommandResult> DispatchAsync<TCommand>(TCommand command, CancellationToken ct) where TCommand : ICommand;
}