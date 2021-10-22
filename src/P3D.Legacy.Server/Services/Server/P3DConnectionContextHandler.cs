﻿using Bedrock.Framework.Protocols;

using MediatR;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Queries.Players;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Application.Services.Connections;
using P3D.Legacy.Server.Infrastructure.Monsters;
using P3D.Legacy.Server.Options;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Server
{
    public sealed partial class P3DConnectionContextHandler : ConnectionContextHandler, IPlayer, IDisposable
    {
        private enum P3DConnectionState { None, Initializing, Intitialized, Finalized }

        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IMediator _mediator;
        private readonly IPlayerContainerReader _playerContainer;
        private readonly IPlayerQueries _playerQueries;
        private readonly P3DProtocol _protocol;
        private readonly WorldService _worldService;
        private readonly ServerOptions _serverOptions;
        private readonly PokeAPIMonsterRepository _monsterRepository;

        private TelemetrySpan _connectionSpan = default!;
        private ProtocolWriter _writer = default!;
        private P3DConnectionState _connectionState = P3DConnectionState.None;

        public P3DConnectionContextHandler(
            ILogger<P3DConnectionContextHandler> logger,
            TracerProvider traceProvider,
            P3DProtocol protocol,
            IPlayerContainerReader playerContainer,
            IPlayerQueries playerQueries,
            WorldService worldService,
            IOptions<ServerOptions> serverOptions,
            IMediator mediator,
            PokeAPIMonsterRepository monsterRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Host");
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _playerQueries = playerQueries ?? throw new ArgumentNullException(nameof(playerQueries));
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
            _serverOptions = serverOptions.Value ?? throw new ArgumentNullException(nameof(serverOptions));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _monsterRepository = monsterRepository ?? throw new ArgumentNullException(nameof(monsterRepository));
        }

        protected override async Task OnCreatedAsync(CancellationToken ct)
        {
            _connectionSpan = _tracer.StartActiveSpan($"P3D Client Connection", SpanKind.Server);

            try
            {
                if (Connection.RemoteEndPoint is IPEndPoint ipEndPoint)
                {
                    IPAddress = ipEndPoint.Address;
                    _connectionSpan.SetAttribute("p3dclient.ipaddress", IPAddress.ToString());
                }

                Features = new FeatureCollection(Connection.Features);
                Features.Set(this as IP3DPlayerState);

                ConnectionId = Features.Get<IConnectionIdFeature>().ConnectionId;

                Features.Get<IConnectionCompleteFeature>().OnCompleted(async _ =>
                {
                    _connectionState = P3DConnectionState.Finalized;
                    var finishSpan = _tracer.StartActiveSpan($"P3D Client Closing", parentContext: _connectionSpan.Context);
                    await _mediator.Send(new PlayerFinalizingCommand(this), CancellationToken.None);
                    finishSpan.End();
                    _connectionSpan.End();
                    finishSpan.Dispose();
                }, default!);

                await using (var reader = Connection.CreateReader())
                await using (_writer = Connection.CreateWriter())
                {
                    var watch = Stopwatch.StartNew();
                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            var result = await reader.ReadAsync(_protocol, ct);
                            await HandlePacketAsync(result.Message, ct);

                            if (result.IsCompleted)
                            {
                                break;
                            }
                        }
                        finally
                        {
                            reader.Advance();
                        }

                        if (_connectionState == P3DConnectionState.Intitialized && watch.ElapsedMilliseconds >= 5000)
                        {
                            await SendPacketAsync(new PingPacket
                            {
                                Origin = Origin.Server
                            }, ct);

                            watch.Restart();
                        }
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
            _connectionSpan.Dispose();
            base.Dispose();
        }
    }
}