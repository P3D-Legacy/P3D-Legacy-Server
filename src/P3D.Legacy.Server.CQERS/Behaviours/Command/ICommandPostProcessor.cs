using P3D.Legacy.Server.CQERS.Commands;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

public interface ICommandPostProcessor<in TCommand> where TCommand : ICommand
{
    Task ProcessAsync(TCommand command, CommandResult result, CancellationToken ct);
}