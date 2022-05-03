using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query
{
    [SuppressMessage("Performance", "CA1812")]
    internal class QueryPerformanceBehaviour<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
    {
        private static readonly Action<ILogger, string, long, TQuery, Exception?> Query = LoggerMessage.Define<string, long, TQuery>(
            LogLevel.Warning, default, "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Query}");

        private readonly Stopwatch _timer;
        private readonly ILogger<TQuery> _logger;

        [SuppressMessage("CodeQuality", "IDE0079")]
        [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
        public QueryPerformanceBehaviour(ILogger<TQuery> logger)
        {
            _timer = new Stopwatch();
            _logger = logger;
        }

        public async Task<TQueryResult> Handle(TQuery query, CancellationToken ct, QueryHandlerDelegate<TQueryResult> next)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var queryName = typeof(TQuery).Name;

                Query(_logger, queryName, elapsedMilliseconds, query, null);
            }

            return response;
        }
    }
}