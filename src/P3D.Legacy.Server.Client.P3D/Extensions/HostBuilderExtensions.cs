using Bedrock.Framework;

using ComposableAsync;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Client.P3D.Options;
using P3D.Legacy.Server.Connections.Extensions;

using RateLimiter;

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.Extensions;

internal sealed partial class ConnectionThrottleMiddleware
{
    [LoggerMessage(Level = LogLevel.Critical, Message = "Client's RemoteEndPoint is not IPEndPoint! {EndPoint}")]
    private partial void InvalidEndPoint(EndPoint? endPoint);

    private readonly TimeLimiter _connectionLimiter = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(300));
    private readonly ConcurrentDictionary<IPNetwork2, TimeLimiter> _subnetLimiter = new(); // TODO: Free after a while

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
            InvalidEndPoint(connectionContext.RemoteEndPoint);
            return;
        }

        // Rate limiting subnet
        var subnet = new IPNetwork2(ipEndPoint.Address, 16);
        await _subnetLimiter.AddOrUpdate(subnet, static _ => TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(2000)), static (_, x) => x);

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

    public static IHostBuilder AddP3DServer(this IHostBuilder hostBuilder) => hostBuilder.ConfigureServer(static (ctx, server) =>
    {
        var p3dServerOptions = server.ApplicationServices.GetRequiredService<IOptions<P3DServerOptions>>().Value;
        server.Listen<SocketTransportFactory>(new IPEndPoint(IPAddress.Parse(p3dServerOptions.IP), p3dServerOptions.Port), static builder => builder
            .UseConnectionThrottle()
            .UseConnectionHandler<P3DConnectionHandler>());
    });
}