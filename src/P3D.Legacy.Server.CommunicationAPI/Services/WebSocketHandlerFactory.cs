using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net.WebSockets;

namespace P3D.Legacy.Server.CommunicationAPI.Services;

public sealed class WebSocketHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public WebSocketHandlerFactory(IServiceProvider serviceProvider)
    {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

    public WebSocketHandler Create(WebSocket webSocket) => ActivatorUtilities.CreateInstance<WebSocketHandler>(_serviceProvider, webSocket);
}