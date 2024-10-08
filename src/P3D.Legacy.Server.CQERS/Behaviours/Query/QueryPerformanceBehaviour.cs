﻿using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Queries;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

internal partial class QueryPerformanceBehaviour<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Query}")]
    private partial void Query(string name, long elapsedMilliseconds, TQuery query);

    private readonly ILogger<TQuery> _logger;
    private readonly Stopwatch _timer = new();

    public int Order => 100;

    [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
    public QueryPerformanceBehaviour(ILogger<TQuery> logger)
    {
        _logger = logger;
    }

    public async Task<TQueryResult> HandleAsync(TQuery query, QueryHandlerDelegate<TQueryResult> next, CancellationToken ct)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var queryName = typeof(TQuery).Name;

            Query(queryName, elapsedMilliseconds, query);
        }

        return response;
    }
}