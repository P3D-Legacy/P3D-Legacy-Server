using Microsoft.Extensions.Logging;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

internal partial class QueryLoggingBehaviour<TQuery> : IQueryPreProcessor<TQuery> where TQuery : notnull
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Query: {Name} {@Query}")]
    private partial void Query(string name, TQuery query);

    private readonly ILogger<TQuery> _logger;

    [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
    public QueryLoggingBehaviour(ILogger<TQuery> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(TQuery query, CancellationToken ct)
    {
        var queryName = typeof(TQuery).Name;

        Query(queryName, query);

        return Task.CompletedTask;
    }
}