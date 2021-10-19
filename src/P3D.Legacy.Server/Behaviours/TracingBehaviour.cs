using MediatR;

using OpenTelemetry.Trace;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Behaviours
{
    internal class TracingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
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