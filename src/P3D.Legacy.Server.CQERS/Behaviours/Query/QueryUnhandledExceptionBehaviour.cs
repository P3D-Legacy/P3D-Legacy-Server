using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Queries;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

internal partial class QueryUnhandledExceptionBehaviour<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
{
#if FALSE // TODO: https://github.com/dotnet/runtime/issues/60968
        [LoggerMessage(Level = LogLevel.Error, Message = "Request: Unhandled Exception for Request {Name} {@Query}")]
#else
    [LoggerMessage(Level = LogLevel.Error, Message = "Request: Unhandled Exception for Request {Name} {Query}")]
#endif
    private partial void Query(string name, TQuery query, Exception exception);

    private readonly ILogger<TQuery> _logger;

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