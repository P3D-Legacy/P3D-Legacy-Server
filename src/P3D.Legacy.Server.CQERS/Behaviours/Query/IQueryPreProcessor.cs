using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

public interface IQueryPreProcessor<in TQuery> where TQuery : notnull
{
    Task ProcessAsync(TQuery query, CancellationToken ct);
}