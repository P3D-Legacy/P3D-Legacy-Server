using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query
{
    [SuppressMessage("Performance", "CA1812")]
    internal class QueryLoggingBehaviour<TQuery> : IQueryPreProcessor<TQuery> where TQuery : notnull
    {
        private static readonly Action<ILogger, string, TQuery, Exception?> Query = LoggerMessage.Define<string, TQuery>(
            LogLevel.Information, default, "Query: {Name} {@Query}");

        private readonly ILogger<TQuery> _logger;

        [SuppressMessage("CodeQuality", "IDE0079")]
        [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
        public QueryLoggingBehaviour(ILogger<TQuery> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(TQuery query, CancellationToken ct)
        {
            var queryName = typeof(TQuery).Name;

            Query(_logger, queryName, query, null);

            return Task.CompletedTask;
        }
    }
}