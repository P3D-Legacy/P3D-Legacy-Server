using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query
{
    [SuppressMessage("Performance", "CA1812")]
    internal class QueryUnhandledExceptionBehaviour<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
    {
        private static readonly Action<ILogger, string, TQuery, Exception?> Query = LoggerMessage.Define<string, TQuery>(
            LogLevel.Error, default, "Request: Unhandled Exception for Request {Name} {@Query}");

        private readonly ILogger<TQuery> _logger;

        [SuppressMessage("CodeQuality", "IDE0079")]
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

                Query(_logger, queryName, query, ex);

                throw;
            }
        }
    }
}