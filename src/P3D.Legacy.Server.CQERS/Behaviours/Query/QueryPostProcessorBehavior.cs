using P3D.Legacy.Server.CQERS.Queries;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query
{
    public class QueryPostProcessorBehavior<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult>
        where TQuery : IQuery<TQueryResult>
    {
        private readonly IEnumerable<IQueryPostProcessor<TQuery, TQueryResult>> _postProcessors;

        public QueryPostProcessorBehavior(IEnumerable<IQueryPostProcessor<TQuery, TQueryResult>> postProcessors)
        {
            _postProcessors = postProcessors;
        }

        public async Task<TQueryResult> Handle(TQuery query, CancellationToken ct, QueryHandlerDelegate<TQueryResult> next)
        {
            var result = await next();

            foreach (var processor in _postProcessors)
            {
                await processor.Process(query, result, ct);
            }

            return result;
        }
    }
}
