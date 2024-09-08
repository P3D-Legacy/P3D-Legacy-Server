using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Queries;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

internal partial class QueryUnhandledExceptionBehaviour<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Request: Unhandled Exception for Request {Name} {@Query}")]
    private partial void Query(string name, TQuery query, Exception exception);

    private readonly ILogger<TQuery> _logger;

    public int Order => 200;

    [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
    public QueryUnhandledExceptionBehaviour(ILogger<TQuery> logger)
    {
        _logger = logger;
    }

    public async Task<TQueryResult> HandleAsync(TQuery query, QueryHandlerDelegate<TQueryResult> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var queryName = typeof(TQuery).Name;

            Query(queryName, query, ex);

            throw;
        }
    }
}