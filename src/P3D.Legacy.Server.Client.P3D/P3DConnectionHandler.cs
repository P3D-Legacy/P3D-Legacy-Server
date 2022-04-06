using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Application.Utils;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    internal sealed class P3DConnectionHandler : ConnectionHandler, IDisposable
    {

        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ConcurrentDictionary<P3DConnectionContextHandler, object?> _connections = new(new ConnectionContextHandlerEqualityComparer());
        private readonly ConcurrentBag<IServiceScope> _connectionScopes = new();

        public P3DConnectionHandler(ILogger<P3DConnectionHandler> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            var connectionScope = _serviceScopeFactory.CreateScope();
            _connectionScopes.Add(connectionScope);

            var connectionContextHandlerFactory = connectionScope.ServiceProvider.GetRequiredService<ConnectionContextHandlerFactory>();

            if (await connectionContextHandlerFactory.CreateAsync<P3DConnectionContextHandler>(connection) is { } connectionContextHandler)
            {
                _connections.TryAdd(connectionContextHandler, null);

                var lifetimeNotificationFeature = connection.Features.Get<IConnectionLifetimeNotificationFeature>();
                var stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(connection.ConnectionClosed, lifetimeNotificationFeature?.ConnectionClosedRequested ?? CancellationToken.None);

                var _ = stoppingCts.Token.Register(() =>
                {
                    _connections.Remove(connectionContextHandler, out var _);
                    stoppingCts.Dispose();
                });
                await connectionContextHandler.ListenAsync();
            }
        }

        public void Dispose()
        {
            foreach (var scope in _connectionScopes)
            {
                scope.Dispose();
            }
        }
    }
}