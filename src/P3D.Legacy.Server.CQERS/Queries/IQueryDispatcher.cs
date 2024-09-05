using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Queries;

public interface IQueryDispatcher
{
    Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken ct);
}