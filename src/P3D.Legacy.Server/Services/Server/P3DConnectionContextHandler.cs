using Bedrock.Framework.Protocols;

using MediatR;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Shared;
using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Models;
using P3D.Legacy.Server.Models.Events;
using P3D.Legacy.Server.Models.Options;
using P3D.Legacy.Server.Notifications;
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

        public Task AssignIdAsync(ulong id, CancellationToken ct)
        {
            if (Id != 0)
                throw new InvalidOperationException("Id was already assigned!");

            Id = id;

            return Task.CompletedTask;
        }

        /*
        public async Task NotifyAsync(Event @event, CancellationToken ct)
        {
            switch (@event)
            {
                case PlayerJoinedEvent(var (id, name, _)):
                    await SendPacketAsync(new CreatePlayerPacket { Origin = Origin.Server, PlayerId = id }, ct);
                    await SendServerMessageAsync($"Player {name} joined the game!", ct);
                    break;

                case PlayerLeavedEvent(var (id, name, _)):
                    if (Id == id) break;
                    await SendPacketAsync(new DestroyPlayerPacket { Origin = Origin.Server, PlayerId = id }, ct);
                    await SendServerMessageAsync($"Player {name} leaved the game!", ct);
                    break;

                case PlayerGameDataEvent(var (id, _, _), var dataItemStorage):
                    if (Id == id) break;
                    await SendPacketAsync(new GameDataPacket { Origin = id, DataItemStorage = { dataItemStorage } }, ct);
                    break;

                case PlayerGlobalMessageEvent(var (id, _, _), var message):
                    await SendPacketAsync(new ChatMessageGlobalPacket { Origin = id, Message = message }, ct);
                    break;

                case PlayerLocalMessageEvent(var (id, _, _), var location, var message):
                    if (LevelFile.Equals(location, StringComparison.OrdinalIgnoreCase))
                        await SendPacketAsync(new ChatMessageGlobalPacket { Origin = id, Message = message }, ct);
                    break;
            }
        }
        */


        private readonly ILogger _logger;
        private readonly P3DProtocol _protocol;
        private readonly IPlayerQueries _playerQueries;
        private readonly WorldService _worldService;
        private readonly ServerOptions _serverOptions;
        private readonly IMediator _mediator;

        private IConnectionLifetimeNotificationFeature _lifetimeNotificationFeature = default!;
        private ProtocolWriter _writer = default!;
        private P3DConnectionState _connectionState = P3DConnectionState.None;

        public P3DConnectionContextHandler(ILogger<P3DConnectionHandler> logger, P3DProtocol protocol, IPlayerQueries playerQueries, WorldService worldService, IOptions<ServerOptions> serverOptions, IMediator mediator)
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
