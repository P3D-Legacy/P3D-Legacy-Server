using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Services;

public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, object?> _cache = new();

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken ct)
    {
        var queryType = query.GetType();
        var handler = GetCached<TQueryResult>(queryType, _serviceProvider);

        return await handler!.DispatchAsync(query, ct);
    }

    private IQueryDispatcherHelper<TQueryResult>? GetCached<TQueryResult>(Type queryType, IServiceProvider serviceProvider) => _cache.GetOrAdd(queryType, static (_, args) =>
    {
        var instanceType = typeof(QueryDispatcherHelper<,>).MakeGenericType(args.queryType, typeof(TQueryResult));
        return args.serviceProvider.GetRequiredService(instanceType);
    }, (queryType, serviceProvider)) as IQueryDispatcherHelper<TQueryResult>;
}