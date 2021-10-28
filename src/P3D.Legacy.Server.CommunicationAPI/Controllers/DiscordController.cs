using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.CommunicationAPI.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CommunicationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly SubscriberManager _subscriberManager;

        public CommunicationController(ILogger<CommunicationController> logger, TracerProvider traceProvider, SubscriberManager subscriberManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.CommunicationAPI");
            _subscriberManager = subscriberManager ?? throw new ArgumentNullException(nameof(subscriberManager));
        }

        [HttpGet("ws/listener")]
        public async Task ListenerAsync(CancellationToken ct)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var connectionSpan = _tracer.StartActiveSpan($"Communication WebSocket Connection", SpanKind.Server);

                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var wrapper = new WebSocketWrapper(webSocket);
                _subscriberManager.Add(wrapper);

                try
                {
                    await wrapper.ListenAsync(ct);
                }
                catch (Exception e)
                {
                    connectionSpan.RecordException(e);
                }
                finally
                {
                    _subscriberManager.Remove(wrapper);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }
}