using P3D.Legacy.Server.CQERS.Behaviours.Query;
using P3D.Legacy.Server.Domain.Queries;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Services;

internal interface IQueryDispatcherHelper<TQueryResult>
{
    Task<TQueryResult> DispatchAsync(IQuery<TQueryResult> rawQuery, CancellationToken ct);
}

internal class QueryDispatcherHelper<TQuery, TQueryResult> : IQueryDispatcherHelper<TQueryResult> where TQuery : class, IQuery<TQueryResult>
{
    private readonly IQueryHandler<TQuery, TQueryResult> _handler;
    private readonly IEnumerable<IQueryBehavior<TQuery, TQueryResult>> _behaviors;

    public QueryDispatcherHelper(IQueryHandler<TQuery, TQueryResult> handler, IEnumerable<IQueryBehavior<TQuery, TQueryResult>> behaviors)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _behaviors = behaviors ?? throw new ArgumentNullException(nameof(behaviors));
    }

    public Task<TQueryResult> DispatchAsync(IQuery<TQueryResult> rawQuery, CancellationToken ct)
    {
        var query = Unsafe.As<TQuery>(rawQuery);
        return DispatchAsync(query, ct);
    }

    private Task<TQueryResult> DispatchAsync(TQuery query, CancellationToken ct)
    {
        Task<TQueryResult> GetHandlerAsync() => _handler.HandleAsync(query, ct);
        return _behaviors
            .Reverse()
            .Aggregate((QueryHandlerDelegate<TQueryResult>) GetHandlerAsync, (next, pipeline) => () => pipeline.HandleAsync(query, next, ct))();
    }
}