using CorrelationId.Abstractions;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Utils
{
    internal sealed class CorrelationIdOverrideHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationIdOverrideHandler(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_correlationContextAccessor.CorrelationContext?.CorrelationId))
            {
                if (request.Headers.Contains(_correlationContextAccessor.CorrelationContext.Header))
                    request.Headers.Remove(_correlationContextAccessor.CorrelationContext.Header);

                request.Headers.Add(_correlationContextAccessor.CorrelationContext.Header, _correlationContextAccessor.CorrelationContext.CorrelationId);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}