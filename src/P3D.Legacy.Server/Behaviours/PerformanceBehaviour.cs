using MediatR;

using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Behaviours
{
    [SuppressMessage("Performance", "CA1812")]
    internal class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private static readonly Action<ILogger, string, long, TRequest, Exception?> Request = LoggerMessage.Define<string, long, TRequest>(
            LogLevel.Warning, default, "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}");

        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;

        [SuppressMessage("CodeQuality", "IDE0079")]
        [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
        public PerformanceBehaviour(ILogger<TRequest> logger)
        {
            _timer = new Stopwatch();
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;

                Request(_logger, requestName, elapsedMilliseconds, request, null);
            }

            return response;
        }
    }
}