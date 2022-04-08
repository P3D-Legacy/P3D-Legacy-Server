using Bedrock.Framework;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Benchmark
{
    public class P3DClientConnectionService
    {
        private static EndPoint GetEndPoint(string host, ushort port) => IPAddress.TryParse(host, out var ipAddress)
            ? new IPEndPoint(ipAddress, port)
            : new DnsEndPoint(host, port);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        //private readonly Bedrock.Framework.Client _client;

        public P3DClientConnectionService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            //_client = new ClientBuilder(_serviceProvider).UseSockets().UseConnectionLogging(loggerFactory: _loggerFactory).Build();
        }

        public async Task<ConnectionContext?> GetConnectionAsync(string host, ushort port, CancellationToken ct)
        {
            var client = new ClientBuilder(_serviceProvider).UseSockets().UseConnectionLogging(loggerFactory: _loggerFactory).Build();
            return await client.ConnectAsync(GetEndPoint(host, port), ct);
        }
    }
}