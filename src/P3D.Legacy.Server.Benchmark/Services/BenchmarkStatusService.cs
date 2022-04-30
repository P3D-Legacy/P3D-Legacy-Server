using Bedrock.Framework.Protocols;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Server.Benchmark.Options;
using P3D.Legacy.Server.Client.P3D;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Benchmark.Services
{
    public sealed class BenchmarkStatusService
    {
        private readonly ILogger _logger;
        private readonly CLIOptions _options;
        private readonly P3DProtocol _protocol;
        private readonly P3DClientConnectionService _connectionService;

        public BenchmarkStatusService(ILogger<BenchmarkStatusService> logger, IOptions<CLIOptions> options, P3DProtocol protocol, P3DClientConnectionService connectionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        }

        private async Task<bool> GetServerStatusAsync(int idx, CancellationToken ct)
        {
            _logger.LogInformation("Iteration: {I}", idx);
            await using var connection = await _connectionService.GetConnectionAsync(_options.Host, _options.Port, ct);

            await using var reader = connection.CreateReader();
            await using var writer = connection.CreateWriter();

            await writer.WriteAsync(_protocol, new ServerDataRequestPacket(), ct);

            if (await reader.ReadAsync(_protocol, ct) is { Message: ServerInfoDataPacket })
            {
                _logger.LogInformation("Iteration: {I} Complete", idx);
                return true;
            }
            reader.Advance();

            _logger.LogInformation("Iteration: {I} Complete", idx);
            return false;
        }

        public async Task ExecuteAsync(CancellationToken ct)
        {
            var batch = _options.Batch;
            _logger.LogInformation("Start. Batch: {Batch}", batch);

            //var desiredThreads = batch * parallel;
            //ThreadPool.GetMaxThreads(out _, out var maxIoThreads);
            //ThreadPool.SetMaxThreads(desiredThreads, maxIoThreads);
            //ThreadPool.GetMinThreads(out _, out var minIoThreads);
            //ThreadPool.SetMinThreads(desiredThreads, minIoThreads);

            var stopwatchGlobal = Stopwatch.StartNew();
            var failedCount = 0;
            await Parallel.ForEachAsync(Enumerable.Range(0, batch), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = ct }, async (i, ct2) =>
            {
                if (!await GetServerStatusAsync(i, ct2)) failedCount++;
            });
            stopwatchGlobal.Stop();
            _logger.LogInformation("End. Time {Time} ms. Failed: {FailedCount}", stopwatchGlobal.ElapsedMilliseconds, failedCount);
        }
    }
}