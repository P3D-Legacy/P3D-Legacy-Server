using Bedrock.Framework;

using ComposableAsync;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Client.P3D.Options;

using RateLimiter;

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    internal sealed class ConnectionThrottleMiddleware
    {
        private static readonly Action<ILogger, EndPoint?, Exception?> InvalidEndPoint = LoggerMessage.Define<EndPoint?>(
            LogLevel.Critical, default, "Client's RemoteEndPoint is not IPEndPoint! {EndPoint}");

        private static readonly IPAddress Netmask = IPAddress.Parse("255.255.0.0");

        private readonly TimeLimiter _connectionLimiter = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(300));
        private readonly ConcurrentDictionary<IPNetwork, TimeLimiter> _subnetLimiter = new(); // TODO: Free after a while

        private readonly ConnectionDelegate _next;
        private readonly ILogger _logger;

        public ConnectionThrottleMiddleware(ILogger<ConnectionThrottleMiddleware> logger, ConnectionDelegate next)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task OnConnectionAsync(ConnectionContext connectionContext)
        {
            if (connectionContext.RemoteEndPoint is not IPEndPoint ipEndPoint)
            {
                InvalidEndPoint(_logger, connectionContext.RemoteEndPoint, null);
                return;
            }

            // Rate limiting subnet
            var subnet = IPNetwork.Parse(ipEndPoint.Address, Netmask);
            await _subnetLimiter.AddOrUpdate(subnet, _ => TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(2000)), (_, x) => x);

            // Rate limiting general
            await _connectionLimiter;

            await _next(connectionContext).ConfigureAwait(false);
        }
    }

    public static class HostBuilderExtensions
    {
        private static TBuilder UseConnectionThrottle<TBuilder>(this TBuilder builder) where TBuilder : IConnectionBuilder
        {
            var logger = builder.ApplicationServices.GetRequiredService<ILogger<ConnectionThrottleMiddleware>>();
            builder.Use(next => new ConnectionThrottleMiddleware(logger, next).OnConnectionAsync);
            return builder;
        }

        public static IHostBuilder AddP3DServer(this IHostBuilder hostBuilder) => hostBuilder.ConfigureServer((ctx, server) =>
        {
            var p3dServerOptions = server.ApplicationServices.GetRequiredService<IOptions<P3DServerOptions>>().Value;
            server.UseSockets(sockets =>
            {
                sockets.Listen(new IPEndPoint(IPAddress.Parse(p3dServerOptions.IP), p3dServerOptions.Port), builder =>
                {
                    builder
                        .UseConnectionThrottle()
                        .UseConnectionHandler<P3DConnectionHandler>();
                });
            });
        });
    }
}