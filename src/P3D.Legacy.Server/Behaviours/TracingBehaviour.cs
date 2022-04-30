using MediatR;

using OpenTelemetry.Trace;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Behaviours
{
    [SuppressMessage("Performance", "CA1812")]
    internal class TracingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly Tracer _tracer;

        public TracingBehaviour(TracerProvider tracerProvider)
        {
            _tracer = tracerProvider.GetTracer("P3D.Legacy.Server.Host");
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
        {
            var span = _tracer.StartActiveSpan($"{typeof(TRequest).Name} Handle");
            try
            {
                return await next();

            }
            catch
            {
                span.SetStatus(Status.Error);
                throw;
            }
            finally
            {
                span.Dispose();
            }
        }
    }
}