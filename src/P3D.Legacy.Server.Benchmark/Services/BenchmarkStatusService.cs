using Bedrock.Framework.Protocols;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Server.Client.P3D;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ZLogger;

namespace P3D.Legacy.Server.Benchmark.Services
{
    public class BenchmarkStatusService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly P3DClientConnectionService _connectionService;

        public BenchmarkStatusService(ILogger<BenchmarkStatusService> logger, P3DClientConnectionService connectionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        }

        private async Task<bool> GetServerStatus(CancellationToken ct)
        {
            string host = "karp.pokemon3d.net";
            ushort port = 15134;
            await using var connection = await _connectionService.GetConnectionAsync(host, port, ct);

            await using var reader = connection.CreateReader();
            await using var writer = connection.CreateWriter();

            var protocol = new P3DProtocol(NullLogger<P3DProtocol>.Instance, new P3DPacketFactory());

            await writer.WriteAsync(protocol, new ServerDataRequestPacket(), ct);

            if (await reader.ReadAsync(protocol, ct) is { Message: { } message, IsCompleted: var isCompleted, IsCanceled: var isCanceled })
            {
                if (message is ServerInfoDataPacket)
                {
                    return true;
                }
            }
            reader.Advance();

            return false;
        }

        private async Task<IEnumerable<bool>> GetConnectionsInParallelInWithBatches(int numberOfBatches, CancellationToken ct)
        {
            var tasks = new List<Task<bool>>();

            for (var i = 0; i < numberOfBatches; i++)
            {
                tasks.Add(GetServerStatus(ct));
            }
            
            return await Task.WhenAll(tasks);
        }

        public async Task StartAsync(CancellationToken ct)
        {
            var batch = 1000;
            var parallel = Environment.ProcessorCount;
            _logger.ZLogInformation("Start. Batch: {Batch}", batch * parallel);
            var stopwatch = Stopwatch.StartNew();
            var failedCount = 0;
            await Parallel.ForEachAsync(Enumerable.Range(0, parallel), ct, async (i, ct) =>
            {
                var stopwatch_p = Stopwatch.StartNew();
                foreach (var result in await GetConnectionsInParallelInWithBatches(batch, ct))
                {
                    if (!result) failedCount++;
                }
                stopwatch_p.Stop();
                _logger.ZLogInformation("Parallel End {Parallel}. Time: {Time} ms.", i, stopwatch_p.ElapsedMilliseconds);
            });
            stopwatch.Stop();
            _logger.ZLogInformation("End. Time {Time} ms. Failed: {FailedCount}", stopwatch.ElapsedMilliseconds, failedCount);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}