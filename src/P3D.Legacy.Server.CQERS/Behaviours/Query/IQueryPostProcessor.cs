using P3D.Legacy.Server.Domain.Queries;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

public interface IQueryPostProcessor<in TQuery, in TQueryResult> where TQuery : IQuery<TQueryResult>
{
    Task ProcessAsync(TQuery query, TQueryResult result, CancellationToken ct);
}