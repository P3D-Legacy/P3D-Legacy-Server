using Bedrock.Framework.Protocols;

using MediatR;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Models;
using P3D.Legacy.Server.Models.Options;
using P3D.Legacy.Server.Queries.Permissions;
using P3D.Legacy.Server.Queries.Players;
using P3D.Legacy.Server.Services.Connections;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Server
{
    public partial class P3DConnectionContextHandler : ConnectionContextHandler, IPlayer
    {
        private enum P3DConnectionState { None, Initializing, Intitialized, Finalized }

        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IPlayerQueries _playerQueries;
        private readonly P3DProtocol _protocol;
        private readonly WorldService _worldService;
        private readonly ServerOptions _serverOptions;

        private IConnectionLifetimeNotificationFeature _lifetimeNotificationFeature = default!;
        private ProtocolWriter _writer = default!;
        private P3DConnectionState _connectionState = P3DConnectionState.None;

        public P3DConnectionContextHandler(
            ILogger<P3DConnectionContextHandler> logger,
            P3DProtocol protocol,
            IPlayerQueries playerQueries,
            WorldService worldService,
            IOptions<ServerOptions> serverOptions,
            IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _playerQueries = playerQueries ?? throw new ArgumentNullException(nameof(playerQueries));
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
            _serverOptions = serverOptions.Value ?? throw new ArgumentNullException(nameof(serverOptions));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected override async Task OnCreatedAsync(CancellationToken ct)
        {
            _lifetimeNotificationFeature = Connection.Features.Get<IConnectionLifetimeNotificationFeature>();

            var connectionIdFeature = Connection.Features.Get<IConnectionIdFeature>();
            ConnectionId = connectionIdFeature.ConnectionId;

            var connectionCompleteFeature = Connection.Features.Get<IConnectionCompleteFeature>();
            connectionCompleteFeature.OnCompleted(async _ =>
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

                    if (_connectionState == P3DConnectionState.Intitialized && watch.ElapsedMilliseconds >= 1000)
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
