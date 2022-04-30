using MediatR;

using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Behaviours
{
    [SuppressMessage("Performance", "CA1812")]
    internal class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private static readonly Action<ILogger, string, TRequest, Exception?> Request = LoggerMessage.Define<string, TRequest>(
            LogLevel.Error, default, "Request: Unhandled Exception for Request {Name} {@Request}");

        private readonly ILogger<TRequest> _logger;

        [SuppressMessage("CodeQuality", "IDE0079")]
        [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
        public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                Request(_logger, requestName, request, ex);

                throw;
            }
        }
    }
}