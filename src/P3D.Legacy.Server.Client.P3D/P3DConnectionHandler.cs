using ComposableAsync;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Application.Utils;

using RateLimiter;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    internal sealed class P3DConnectionHandler : ConnectionHandler, IDisposable
    {
        private static readonly IPAddress Netmask = IPAddress.Parse("255.255.0.0");

        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ConcurrentDictionary<P3DConnectionContextHandler, object?> _connections = new(new ConnectionContextHandlerEqualityComparer());
        private readonly ConcurrentBag<IServiceScope> _connectionScopes = new();

        private readonly TimeLimiter _connectionLimiter = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(300));
        private readonly ConcurrentDictionary<IPNetwork, TimeLimiter> _subnetLimiter = new(); // TODO: Free after a while

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
                // Rate limiting subnet
                if (connection.RemoteEndPoint is IPEndPoint ipEndPoint)
                {
                    var subnet = IPNetwork.Parse(ipEndPoint.Address, Netmask);
                    await _subnetLimiter.AddOrUpdate(subnet, _ => TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(2000)), (_, x) => x);
                }

                // Rate limiting general
                await _connectionLimiter;


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