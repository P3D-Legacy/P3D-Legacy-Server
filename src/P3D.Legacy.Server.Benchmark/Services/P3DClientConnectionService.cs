using Bedrock.Framework;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Benchmark.Services
{
    public class P3DClientConnectionService
    {
        private static EndPoint GetEndPoint(string host, ushort port) => IPAddress.TryParse(host, out var ipAddress)
            ? new IPEndPoint(ipAddress, port)
            : new DnsEndPoint(host, port);

        private readonly Bedrock.Framework.Client _client;

        public P3DClientConnectionService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _client = new ClientBuilder(serviceProvider).UseSockets().UseConnectionLogging(loggerFactory: loggerFactory).Build();
        }

        public async Task<ConnectionContext?> GetConnectionAsync(string host, ushort port, CancellationToken ct) => await _client.ConnectAsync(GetEndPoint(host, port), ct);
    }
}