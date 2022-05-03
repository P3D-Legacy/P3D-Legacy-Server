using P3D.Legacy.Server.CQERS.Commands;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command
{
    public interface ICommandBehavior<in TCommand> where TCommand : ICommand
    {
        Task<CommandResult> Handle(TCommand command, CancellationToken ct, CommandHandlerDelegate next);
    }
}