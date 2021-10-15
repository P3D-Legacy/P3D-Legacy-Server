using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Services.Connections;
using P3D.Legacy.Server.Services.Server;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public sealed class P3DConnectionHandler : ConnectionHandler, IDisposable
    {
        private class P3DConnectionContextHandlerEqualityComparer : IEqualityComparer<P3DConnectionContextHandler>
        {
            public bool Equals(P3DConnectionContextHandler? x, P3DConnectionContextHandler? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.ConnectionId == y.ConnectionId;
            }

            public int GetHashCode(P3DConnectionContextHandler obj) => obj.ConnectionId.GetHashCode();
        }

        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly HashSet<P3DConnectionContextHandler> _connections = new(new P3DConnectionContextHandlerEqualityComparer());
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
                _connections.Add(connectionContextHandler);

                var lifetimeNotificationFeature = connection.Features.Get<IConnectionLifetimeNotificationFeature>();
                var stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(connection.ConnectionClosed, lifetimeNotificationFeature.ConnectionClosedRequested);

                var _ = stoppingCts.Token.Register(() =>
                {
                    _connections.Remove(connectionContextHandler);
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