using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.CommunicationAPI.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CommunicationAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public sealed class CommunicationController : ControllerBase
    {
        private static readonly Action<ILogger, Exception?> ConnectionReceived = LoggerMessage.Define(
            LogLevel.Trace, default, "WebSocket connection received at 'listener/ws'");

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
                ConnectionReceived(_logger, null);

                using var connectionSpan = _tracer.StartActiveSpan("Communication WebSocket Connection", SpanKind.Server);

                await using var handler = _handlerFactory.Create(await HttpContext.WebSockets.AcceptWebSocketAsync("json"));
                _subscribtionManager.AddOrUpdate(HttpContext.Connection.Id, static (_, x) => x, static (_, _, x) => x, handler);

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