using MediatR;

using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions.Services;

using System;
using System.Net.WebSockets;
using System.Text.Json;

namespace P3D.Legacy.Server.CommunicationAPI.Services
{
    public sealed class WebSocketHandlerFactory
    {
        private readonly IMediator _mediator;
        private readonly NotificationPublisher _notificationPublisher;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public WebSocketHandlerFactory(IMediator mediator, NotificationPublisher notificationPublisher, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
            _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public WebSocketHandler Create(WebSocket webSocket) => new(webSocket, _mediator, _notificationPublisher, _jsonSerializerOptions);
    }
}