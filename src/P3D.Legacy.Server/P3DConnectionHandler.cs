using Bedrock.Framework.Protocols;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Common.Packets.Battle;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Shared;
using P3D.Legacy.Common.Packets.Trade;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Services;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections.Features;

namespace P3D.Legacy.Server
{
    public enum P3DConnectionState { None, Initializing, Intitialized, Finalized }

    public partial class P3DConnectionHandler : ConnectionHandler, IPlayer
    {
        public event Func<string, P3DConnectionHandler, Task>? InitializingAsync;
        public event Func<string, P3DConnectionHandler, Task>? InitializedAsync;
        public event Func<string, P3DConnectionHandler, Task>? DisconnectedAsync;
        public Task AssignIdAsync(uint id)
        {
            Id = id;
            return Task.CompletedTask;
        }


        private readonly ILogger _logger;
        private readonly P3DProtocol _protocol;
        private readonly PlayerHandlerService _playerHandlerService;
        private readonly WorldService _worldService;
        private readonly ServerOptions _serverOptions;

        private IConnectionIdFeature _connectionIdFeature;
        private IConnectionLifetimeNotificationFeature _lifetimeNotificationFeature = default!;

        private CancellationTokenSource _cancellationTokenSource = default!;
        private ProtocolWriter _writer = default!;
        private P3DConnectionState _connectionState = P3DConnectionState.None;

        public P3DConnectionHandler(
            ILogger<P3DConnectionHandler> logger,
            P3DProtocol protocol,
            PlayerHandlerService playerHandlerService,
            WorldService worldService,
            IOptions<ServerOptions> serverOptions
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _playerHandlerService = playerHandlerService ?? throw new ArgumentNullException(nameof(playerHandlerService));
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
            _serverOptions = serverOptions.Value ?? throw new ArgumentNullException(nameof(serverOptions));
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            // ASP.NET Core is reusing the connection
            if (_connectionState == P3DConnectionState.Finalized)
            {
                _connectionState = P3DConnectionState.None;
            }

            _connectionIdFeature = connection.Features.Get<IConnectionIdFeature>();
            _lifetimeNotificationFeature = connection.Features.Get<IConnectionLifetimeNotificationFeature>();

            await _playerHandlerService.AcknowledgeConnectionAsync(_connectionIdFeature.ConnectionId, this);

            var connectionCompleteFeature = connection.Features.Get<IConnectionCompleteFeature>();
            connectionCompleteFeature.OnCompleted(async _ =>
            {
                _connectionState = P3DConnectionState.Finalized;
                if (DisconnectedAsync is not null)
                    await DisconnectedAsync(_connectionIdFeature.ConnectionId, this);
            }, null!);

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeNotificationFeature.ConnectionClosedRequested);
            var ct = _cancellationTokenSource.Token;

            await using (var reader = connection.CreateReader())
            await using (_writer = connection.CreateWriter())
            {
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
                }
            }
        }
    }
}