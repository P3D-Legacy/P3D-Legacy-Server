using P3D.Legacy.Server.Domain.Queries;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

public class QueryPostProcessorBehavior<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult>
    where TQuery : IQuery<TQueryResult>
{
    private readonly IEnumerable<IQueryPostProcessor<TQuery, TQueryResult>> _postProcessors;

    public int Order => int.MaxValue;

    public QueryPostProcessorBehavior(IEnumerable<IQueryPostProcessor<TQuery, TQueryResult>> postProcessors)
    {
        _postProcessors = postProcessors;
    }

    public async Task<TQueryResult> HandleAsync(TQuery query, QueryHandlerDelegate<TQueryResult> next, CancellationToken ct)
    {
        var result = await next();

        foreach (var processor in _postProcessors)
        {
            await processor.ProcessAsync(query, result, ct);
        }

        return result;
    }
}