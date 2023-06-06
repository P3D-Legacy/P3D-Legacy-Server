﻿using Bedrock.Framework.Protocols;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Benchmark.Options;
using P3D.Legacy.Server.Client.P3D;
using P3D.Legacy.Server.Client.P3D.Packets;
using P3D.Legacy.Server.Client.P3D.Packets.Chat;
using P3D.Legacy.Server.Client.P3D.Packets.Common;
using P3D.Legacy.Server.Client.P3D.Packets.Server;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Benchmark.Services
{
    public sealed class BenchmarkService
    {
        private sealed class PlayerState
        {
            public string Name { get; set; } = "";
            public Origin Origin { get; set; } = Origin.FromNumber(-1);

            public Func<P3DPacket, ValueTask> SendMessage { get; set; } = static _ => ValueTask.CompletedTask;
        }

        private readonly ILogger _logger;
        private readonly CLIOptions _options;
        private readonly P3DClientConnectionService _connectionService;
        private readonly Random _random = new();

        public BenchmarkService(ILogger<BenchmarkService> logger, IOptions<CLIOptions> options, P3DClientConnectionService connectionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        }

        private async Task<Exception?> GetAsync(CancellationToken ct2)
        {
            using var cts = new CancellationTokenSource(10000);
            using var cts2 = CancellationTokenSource.CreateLinkedTokenSource(ct2, cts.Token);
            var ct = cts2.Token;

            try
            {
                await using var connection = await _connectionService.GetConnectionAsync(_options.Host, _options.Port, ct);
                if (connection is null) return new NullReferenceException();

                await using var reader = connection.CreateReader();
                await using var writer = connection.CreateWriter();

                var protocol = new P3DProtocol(NullLogger<P3DProtocol>.Instance, new P3DPacketServerFactory());

                var state = new PlayerState
                {
                    Name = $"Player_{_random.Next()}",
                    SendMessage = async message =>
                    {
                        await writer.WriteAsync(protocol, message, ct);
                        await connection.Transport.Output.FlushAsync(ct);
                    },
                };
                var gameData = new GameDataPacket
                {
                    Origin = state.Origin,
                    GameMode = "Benchmark",
                    IsGameJoltPlayer = false,
                    GameJoltId = 0,
                    DecimalSeparator = '.',
                    Name = state.Name,
                    LevelFile = "benchmark",
                    Position = Vector3.Zero,
                    Facing = 1,
                    Moving = false,
                    Skin = "",
                    BusyType = "",
                    MonsterVisible = false,
                    MonsterPosition = Vector3.Zero,
                    MonsterSkin = "",
                    MonsterFacing = 1
                };
                await writer.WriteAsync(protocol, gameData, ct);
                await connection.Transport.Output.FlushAsync(ct);

                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        if (await reader.ReadAsync(protocol, ct) is { Message: { } message, IsCompleted: var isCompleted, IsCanceled: var isCanceled })
                        {
                            switch (await HandlePacketAsync(state, message, ct))
                            {
                                case Result.Continue:
                                    continue;
                                case Result.Success:
                                    return null;
                                case Result.Error:
                                    return new Exception("Result.Error");
                            }

                            if (isCompleted || isCanceled)
                                return new Exception("isCompleted || isCanceled");
                        }
                    }
                    finally
                    {
                        reader.Advance();
                    }
                }

                connection.Features.Get<IConnectionLifetimeNotificationFeature>()?.RequestClose();
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private enum Result
        {
            Continue,
            Success,
            Error,
        }
        private static async Task<Result> HandlePacketAsync(PlayerState state, P3DPacket? packet, CancellationToken ct)
        {
            if (packet is null) return Result.Error;

            switch (packet)
            {
                case ChatMessageGlobalPacket chatMessageGlobalPacket:
                    var expected1 = $"Player {state.Name} joined the server!";
                    var expected2 = "The game client sends a SHA-512 hash instead of the raw password.";
                    if (string.Equals(chatMessageGlobalPacket.Message, expected2, StringComparison.Ordinal))
                    {
                        await state.SendMessage(new ChatMessageGlobalPacket { Origin = state.Origin, Message = "/login 12345Ara!" });
                    }
                    if (string.Equals(chatMessageGlobalPacket.Message, expected1, StringComparison.Ordinal))
                    {
                        return Result.Success;
                    }
                    break;


                case IdPacket idPacket:
                    state.Origin = idPacket.PlayerOrigin;
                    break;
                case WorldDataPacket:
                    break;
            }

            return Result.Continue;
        }

        private async Task<IEnumerable<Exception?>> GetConnectionsInParallelInWithBatchesAsync(int numberOfBatches, CancellationToken ct)
        {
            var tasks = new List<Task<Exception?>>();

            for (var i = 0; i < numberOfBatches; i++)
            {
                tasks.Add(GetAsync(ct));
            }

            return await Task.WhenAll(tasks);
        }

        public async Task ExecuteAsync(CancellationToken ct)
        {
            var batch = 20;
            var parallel = Environment.ProcessorCount;
            _logger.LogInformation("Start. Batch: {Batch}", batch * parallel);
            var stopwatch = Stopwatch.StartNew();
            var failedCount = 0;
            await Parallel.ForEachAsync(Enumerable.Range(0, parallel), ct, async (i, ct2) =>
            {
                var stopwatch_p = Stopwatch.StartNew();
                foreach (var result in await GetConnectionsInParallelInWithBatchesAsync(batch, ct2))
                {
                    if (result is not null)
                    {
                        if (result is not OperationCanceledException)
                            _logger.LogError(result, "Parallel {Parallel}", i);
                        failedCount++;
                    }
                }
                stopwatch_p.Stop();
                _logger.LogInformation("Parallel End {Parallel}. Time: {Time} ms", i, stopwatch_p.ElapsedMilliseconds);
            });
            stopwatch.Stop();
            _logger.LogInformation("End. Time {Time} ms. Failed: {FailedCount}", stopwatch.ElapsedMilliseconds, failedCount);
            /*
            var batch = 4;
            var parallel = 6;
            var count = 10;
            _logger.LogInformation("Start. Batch: {Batch}", batch * parallel);
            var stopwatch = Stopwatch.StartNew();
            var failedCount = 0;
            var executeCount = 0;
            for (var i = 0; i < count; i++)
            {
                new Thread(() =>
                {
                    var stopwatch_p = Stopwatch.StartNew();
                    foreach (var result in GetConnectionsInParallelInWithBatches(batch, ct).GetAwaiter().GetResult())
                    {
                        if (!result) Interlocked.Increment(ref failedCount);
                    }
                    stopwatch_p.Stop();
                    _logger.LogInformation("Parallel End {Parallel}. Time: {Time} ms.", i, stopwatch_p.ElapsedMilliseconds);
                    Interlocked.Increment(ref executeCount);
                }).Start();
            }
            while (executeCount != count) { Thread.Sleep(100);  }
            stopwatch.Stop();
            _logger.LogInformation("End. Time {Time} ms. Failed: {FailedCount}", stopwatch.ElapsedMilliseconds, failedCount);
            */
        }
    }
}