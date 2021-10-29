using MediatR.Pipeline;

using Microsoft.Extensions.Logging;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Behaviours
{
    internal class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
    {
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

            _logger.LogInformation("Request: {Name} {@Request}", requestName, request);

            return Task.CompletedTask;
        }
    }
}