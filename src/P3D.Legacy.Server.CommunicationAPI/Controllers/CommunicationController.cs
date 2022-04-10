using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.CommunicationAPI.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ZLogger;

namespace P3D.Legacy.Server.CommunicationAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CommunicationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly WebSocketHandlerFactory _handlerFactory;
        private readonly WebSocketSubscribtionManager _subscribtionManager;

        public CommunicationController(ILogger<CommunicationController> logger, TracerProvider traceProvider, WebSocketHandlerFactory handlerFactory, WebSocketSubscribtionManager subscribtionManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.CommunicationAPI");
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
            _subscribtionManager = subscribtionManager ?? throw new ArgumentNullException(nameof(subscribtionManager));
        }

        [HttpGet("listener/ws")]
        public async Task ListenerAsync(CancellationToken ct)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.ZLogTrace("WebSocket connection received at 'listener/ws'");

                using var connectionSpan = _tracer.StartActiveSpan("Communication WebSocket Connection", SpanKind.Server);

                await using var handler = _handlerFactory.Create(await HttpContext.WebSockets.AcceptWebSocketAsync("json"));
                // ReSharper disable AccessToDisposedClosure
                _subscribtionManager.AddOrUpdate(HttpContext.Connection.Id, _ => handler, (_, _) => handler);
                // ReSharper restore AccessToDisposedClosure

                try
                {
                    await handler.ListenAsync(ct);
                }
                catch (Exception e)
                {
                    connectionSpan.RecordException(e);
                }
                finally
                {
                    _subscribtionManager.Remove(HttpContext.Connection.Id, out _);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}