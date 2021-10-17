using Bedrock.Framework.Protocols;

using MediatR;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Queries.Players;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Application.Services.Connections;
using P3D.Legacy.Server.Options;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Server
{
    public partial class P3DConnectionContextHandler : ConnectionContextHandler, IPlayer
    {
        private enum P3DConnectionState { None, Initializing, Intitialized, Finalized }

        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IPlayerContainerReader _playerContainer;
        private readonly IPlayerQueries _playerQueries;
        private readonly P3DProtocol _protocol;
        private readonly WorldService _worldService;
        private readonly ServerOptions _serverOptions;

        private ProtocolWriter _writer = default!;
        private P3DConnectionState _connectionState = P3DConnectionState.None;

        public P3DConnectionContextHandler(
            ILogger<P3DConnectionContextHandler> logger,
            P3DProtocol protocol,
            IPlayerContainerReader playerContainer,
            IPlayerQueries playerQueries,
            WorldService worldService,
            IOptions<ServerOptions> serverOptions,
            IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _playerQueries = playerQueries ?? throw new ArgumentNullException(nameof(playerQueries));
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
            _serverOptions = serverOptions.Value ?? throw new ArgumentNullException(nameof(serverOptions));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected override async Task OnCreatedAsync(CancellationToken ct)
        {
            if (Connection.RemoteEndPoint is IPEndPoint ipEndPoint)
                IPAddress = ipEndPoint.Address;

            Features = new FeatureCollection(Connection.Features);
            Features.Set(this as IP3DPlayerState);

            ConnectionId = Features.Get<IConnectionIdFeature>().ConnectionId;

            Features.Get<IConnectionCompleteFeature>().OnCompleted(async _ =>
            {
                _connectionState = P3DConnectionState.Finalized;
                await _mediator.Send(new PlayerFinalizingCommand(this), CancellationToken.None);
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
    }
}