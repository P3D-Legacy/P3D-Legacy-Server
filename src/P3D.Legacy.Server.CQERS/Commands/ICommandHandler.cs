using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Commands
{
    public interface ICommandHandler { }
    public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : ICommand
    {
        Task<CommandResult> HandleAsync(TCommand command, CancellationToken ct);
    }
}