using Bedrock.Framework.Protocols;

using MediatR;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Infrastructure.Repositories.Monsters;
using P3D.Legacy.Server.Infrastructure.Services.Mutes;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    internal sealed partial class P3DConnectionContextHandler : ConnectionContextHandler, IPlayer
    {
        private enum P3DConnectionState { None, Initializing, Authentication, Intitialized, Finalized }

        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IMediator _mediator;
        private readonly IPlayerContainerReader _playerContainer;
        private readonly P3DProtocol _protocol;
        private readonly WorldService _worldService;
        private readonly ServerOptions _serverOptions;
        private readonly IMonsterRepository _monsterRepository;
        private readonly IMuteManager _muteManager;
        private readonly IMemoryCache _memoryCache;

        private TelemetrySpan _connectionSpan = default!;
        private ProtocolWriter _writer = default!;
        private P3DConnectionState _connectionState = P3DConnectionState.None;

        public P3DConnectionContextHandler(
            ILogger<P3DConnectionContextHandler> logger,
            TracerProvider traceProvider,
            P3DProtocol protocol,
            IPlayerContainerReader playerContainer,
            WorldService worldService,
            IOptions<ServerOptions> serverOptions,
            IMediator mediator,
            IMonsterRepository monsterRepository,
            IMuteManager muteManager,
            IMemoryCache memoryCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Client.P3D");
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
            _serverOptions = serverOptions.Value ?? throw new ArgumentNullException(nameof(serverOptions));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _monsterRepository = monsterRepository ?? throw new ArgumentNullException(nameof(monsterRepository));
            _muteManager = muteManager ?? throw new ArgumentNullException(nameof(muteManager));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        protected override async Task OnCreatedAsync(CancellationToken ct)
        {
            _connectionSpan = _tracer.StartActiveSpan("P3D Session UNKNOWN");

            try
            {
                if (Connection.RemoteEndPoint is IPEndPoint ipEndPoint)
                {
                    IPEndPoint = ipEndPoint;
                }

                Connection.Features.Get<IConnectionCompleteFeature>().OnCompleted(async _ =>
                {
                    using var finishSpan = _tracer.StartActiveSpan("P3D Client Closing", parentContext: _connectionSpan.Context);
                    _connectionState = P3DConnectionState.Finalized;
                    await _mediator.Send(new PlayerFinalizingCommand(this), CancellationToken.None);
                }, default!);

                _writer = Connection.CreateWriter();

                await using var reader = Connection.CreateReader();
                var watch = Stopwatch.StartNew();
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        using var span = _tracer.StartActiveSpan("P3D Client Reading Packet", SpanKind.Server);
                        span.SetAttribute("net.peer.ip", IPEndPoint.Address.ToString());
                        span.SetAttribute("net.peer.port", IPEndPoint.Port);
                        span.SetAttribute("net.transport", "ip_tcp");

                        if (await reader.ReadAsync(_protocol, ct) is { Message: { } message, IsCompleted: var isCompleted, IsCanceled: var isCanceled })
                        {
                            span.UpdateName($"P3D Client Reading {message.GetType().FullName}");
                            span.SetAttribute("p3dclient.packet_type", message.GetType().FullName);
                            await HandlePacketAsync(message, ct);

                            if (isCompleted || isCanceled)
                            {
                                break;
                            }
                        }
                    }
                    finally
                    {
                        reader.Advance();
                    }

                    if (_connectionState == P3DConnectionState.Intitialized && watch.ElapsedMilliseconds >= 5000)
                    {
                        await SendPacketAsync(new PingPacket { Origin = Origin.Server }, ct);
                        watch.Restart();
                    }
                }
            }
            catch (Exception e)
            {
                _connectionSpan.RecordException(e).SetStatus(Status.Error);
                throw;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _connectionState = P3DConnectionState.None;
            _protocol.Dispose();
            _connectionSpan.Dispose();
            _writer.DisposeAsync().GetAwaiter().GetResult();

        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            _connectionState = P3DConnectionState.None;
            _protocol.Dispose();
            _connectionSpan.Dispose();
            await _writer.DisposeAsync();
        }
    }
}