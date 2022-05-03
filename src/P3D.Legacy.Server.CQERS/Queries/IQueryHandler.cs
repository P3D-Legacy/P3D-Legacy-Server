using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Queries
{
    public interface IQueryHandler { }
    public interface IQueryHandler<in TQuery, TQueryResult> : IQueryHandler where TQuery : IQuery<TQueryResult>
    {
        Task<TQueryResult> HandleAsync(TQuery query, CancellationToken ct);
    }
}