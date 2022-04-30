using MediatR.Pipeline;

using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Behaviours
{
    [SuppressMessage("Performance", "CA1812")]
    internal class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
    {
        private static readonly Action<ILogger, string, TRequest, Exception?> Request = LoggerMessage.Define<string, TRequest>(
            LogLevel.Information, default, "Request: {Name} {@Request}");

        private readonly ILogger<TRequest> _logger;

        [SuppressMessage("CodeQuality", "IDE0079")]
        [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
        public LoggingBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public Task Process(TRequest request, CancellationToken ct)
        {
            var requestName = typeof(TRequest).Name;

            Request(_logger, requestName, request, null);

            return Task.CompletedTask;
        }
    }
}