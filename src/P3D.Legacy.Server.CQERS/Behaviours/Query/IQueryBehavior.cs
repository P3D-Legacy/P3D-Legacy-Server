using P3D.Legacy.Server.CQERS.Queries;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query
{
    public interface IQueryBehavior<in TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
    {
        Task<TQueryResult> Handle(TQuery query, CancellationToken ct, QueryHandlerDelegate<TQueryResult> next);
    }
}