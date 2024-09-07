using Bedrock.Framework.Protocols;

using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Client.P3D.Packets.Client;
using P3D.Legacy.Server.Client.P3D.Services;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D;

internal sealed partial class P3DConnectionContextHandler : ConnectionContextHandler, IPlayer
{
    private readonly ILogger _logger;
    private readonly Tracer _tracer;
    private readonly P3DProtocol _protocol;
    private readonly P3DMonsterConverter _p3dMonsterConverter;
    private readonly IMonsterValidator _monsterValidator;
    private readonly P3DPlayerMovementCompensationService _movementCompensationService;
    private readonly TaskCompletionSource _finalizationDelayer = new();

    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly IEventDispatcher _eventDispatcher;

    private TelemetrySpan _connectionSpan = default!;
    private ProtocolWriter _writer = default!;


    public P3DConnectionContextHandler(
        ILogger<P3DConnectionContextHandler> logger,
        TracerProvider traceProvider,
        P3DProtocol protocol,
        P3DMonsterConverter p3dMonsterConverter,
        IMonsterValidator monsterValidator,
        P3DPlayerMovementCompensationService movementCompensationService,
        ICommandDispatcher commandDispatcher,
        IQueryDispatcher queryDispatcher,
        IEventDispatcher eventDispatcher)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Client.P3D");
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        _p3dMonsterConverter = p3dMonsterConverter ?? throw new ArgumentNullException(nameof(p3dMonsterConverter));
        _monsterValidator = monsterValidator ?? throw new ArgumentNullException(nameof(monsterValidator));
        _movementCompensationService = movementCompensationService ?? throw new ArgumentNullException(nameof(movementCompensationService));
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning")]
    protected override async Task OnCreatedAsync(CancellationToken ct)
    {
        _connectionSpan = _tracer.StartActiveSpan("P3D Session UNKNOWN");

        try
        {
            if (Connection.RemoteEndPoint is IPEndPoint ipEndPoint)
            {
                IPEndPoint = ipEndPoint;
            }

            _writer = Connection.CreateWriter();
            await using var _ = _writer;

            await using var reader = Connection.CreateReader();
            var watch = Stopwatch.StartNew();
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (await reader.ReadAsync(_protocol, ct) is { Message: { } packet, IsCompleted: var isCompleted, IsCanceled: var isCanceled })
                    {
                        using var span = _tracer.StartActiveSpan($"P3D Client Receiving {packet.GetType().Name}", SpanKind.Server);
                        span.SetAttribute("server.address", IPEndPoint.Address.ToString());
                        span.SetAttribute("server.port", IPEndPoint.Port);
                        span.SetAttribute("network.transport", "tcp");
                        span.SetAttribute("network.protocol.name", "p3d");
                        span.SetAttribute("network.protocol.version", packet.Protocol.ToString());
                        span.SetAttribute("enduser.id", Name);
                        span.SetAttribute("enduser.role", Permissions.ToString());
                        span.SetAttribute("p3dclient.packet_type", packet.GetType().FullName);
                        span.SetAttribute("peer.service", $"{Name} (P3D-Legacy)");

                        // Do not trace the ping packet
                        if (packet is PingPacket && Activity.Current is not null)
                            Activity.Current.IsAllDataRequested = false;

                        await HandlePacketAsync(packet, ct);

                        if (isCompleted || isCanceled)
                            break;
                    }
                }
                finally
                {
                    reader.Advance();
                }

                if (State == PlayerState.Initialized && watch.ElapsedMilliseconds >= 5000)
                {
                    await SendPacketAsync(new PingPacket { Origin = Origin.Server }, ct);
                    watch.Restart();
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _connectionSpan.RecordException(e).SetStatus(Status.Error);
            throw;
        }

        // Exit means disposal. Delay disposal of the client. Wait for finalization if possible
        await _finalizationDelayer.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
    }

    protected override async Task OnConnectionClosedAsync(ConnectionContextHandler connectionContextHandler)
    {
        if (connectionContextHandler is not P3DConnectionContextHandler connection) return;

        using var finishSpan = connection._tracer.StartActiveSpan("P3D Client Closing", SpanKind.Internal, parentSpan: connection._connectionSpan);
        var oldState = connection.State;
        connection.State = PlayerState.Finalizing;
        if (oldState == PlayerState.Initialized) // If the sever initialized the player, make others aware of finalization
        {
            // Start finalization. Here it should complete _finalizationDelayer.
            async Task<CommandResult> FinalizeAsync() =>
                await connection._commandDispatcher.DispatchAsync(new PlayerFinalizingCommand(connection), CancellationToken.None);

            await FinalizeAsync();
        }

        connection.State = PlayerState.Finalized;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            State = PlayerState.None;
            _connectionSpan.Dispose();
        }

        base.Dispose(disposing);
    }
}