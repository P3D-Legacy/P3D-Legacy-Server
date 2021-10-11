using Bedrock.Framework.Protocols;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Services;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public enum P3DConnectionState { None, Initializing, Intitialized, Finalized }

    public partial class P3DConnectionHandler : ConnectionHandler, IPlayer
    {
        public event Func<string, P3DConnectionHandler, Task>? OnInitializingAsync;
        public event Func<string, P3DConnectionHandler, Task>? OnInitializedAsync;
        public event Func<string, P3DConnectionHandler, Task>? OnDisconnectedAsync;
        public Task AssignIdAsync(uint id)
        {
            if (Id != 0)
                throw new InvalidOperationException("Id was already assigned!");

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

#pragma warning disable CS8618
        public P3DConnectionHandler(
#pragma warning restore CS8618
            ILogger<P3DConnectionHandler> logger,
            P3DProtocol protocol,
            PlayerHandlerService playerHandlerService,
            WorldService worldService,
            IOptions<ServerOptions> serverOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _playerHandlerService = playerHandlerService ?? throw new ArgumentNullException(nameof(playerHandlerService));
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
            _serverOptions = serverOptions.Value ?? throw new ArgumentNullException(nameof(serverOptions));

            ResetState();
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            // ASP.NET Core is reusing the connection
            if (_connectionState == P3DConnectionState.Finalized)
            {
                _connectionState = P3DConnectionState.None;
                _playerHandlerService.OnEventAsync -= PlayerHandlerService_OnEventAsync;
            }

            if (_connectionState == P3DConnectionState.None)
            {
                ResetState();
            }

            _connectionIdFeature = connection.Features.Get<IConnectionIdFeature>();
            _lifetimeNotificationFeature = connection.Features.Get<IConnectionLifetimeNotificationFeature>();

            await _playerHandlerService.AcknowledgeConnectionAsync(_connectionIdFeature.ConnectionId, this);

            var connectionCompleteFeature = connection.Features.Get<IConnectionCompleteFeature>();
            connectionCompleteFeature.OnCompleted(async connectionId =>
            {
                _connectionState = P3DConnectionState.Finalized;
                if (OnDisconnectedAsync is not null)
                    await OnDisconnectedAsync((string) connectionId, this);
            }, _connectionIdFeature.ConnectionId);

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(connection.ConnectionClosed, _lifetimeNotificationFeature.ConnectionClosedRequested);
            var ct = _cancellationTokenSource.Token;

            await using (var reader = connection.CreateReader())
            await using (_writer = connection.CreateWriter())
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
                        await _writer.WriteAsync(_protocol, new PingPacket { Origin = Origin.Server}, ct);

                        watch.Restart();
                    }
                }
            }
        }
    }
}