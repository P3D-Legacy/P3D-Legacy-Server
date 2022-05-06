using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command
{
    public interface ICommandPreProcessor<in TCommand> where TCommand : notnull
    {
        Task ProcessAsync(TCommand command, CancellationToken ct);
    }
}