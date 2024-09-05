using P3D.Legacy.Server.CQERS.Queries;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

public class QueryPreProcessorBehavior<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult>
    where TQuery : IQuery<TQueryResult>
{
    private readonly IEnumerable<IQueryPreProcessor<TQuery>> _preProcessors;

    public QueryPreProcessorBehavior(IEnumerable<IQueryPreProcessor<TQuery>> preProcessors)
    {
        _preProcessors = preProcessors;
    }

    public async Task<TQueryResult> HandleAsync(TQuery request, QueryHandlerDelegate<TQueryResult> next, CancellationToken ct)
    {
        foreach (var processor in _preProcessors)
        {
            await processor.ProcessAsync(request, ct);
        }

        return await next();
    }
}