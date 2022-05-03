using P3D.Legacy.Server.CQERS.Queries;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query
{
    public interface IQueryPostProcessor<in TQuery, in TQueryResult> where TQuery : IQuery<TQueryResult>
    {
        Task Process(TQuery query, TQueryResult result, CancellationToken ct);
    }
}