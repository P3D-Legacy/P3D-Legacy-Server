using MediatR;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Notifications;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Server
{
    public partial class P3DConnectionContextHandler :
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeavedNotification>,
        INotificationHandler<PlayerUpdatedStateNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<PlayerSentLocalMessageNotification>,
        INotificationHandler<PlayerSentPrivateMessageNotification>,
        INotificationHandler<MessageToPlayerNotification>,
        INotificationHandler<PlayerSentRawP3DPacketNotification>,
        INotificationHandler<ServerMessageNotification>
    {
        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            var player = notification.Player;

            if (Id == player.Id)
            {
                await foreach (var connectedPlayer in _playerContainer.GetAllAsync(ct))
                {
                    if (connectedPlayer.Features.Get<IP3DPlayerState>() is { } state)
                    {
                        await SendPacketAsync(new CreatePlayerPacket { Origin = Origin.Server, PlayerId = connectedPlayer.Id }, ct);
                        await SendPacketAsync(GetFromP3DPlayerState(connectedPlayer, state), ct);
                    }
                }
            }
            else
            {
                await SendPacketAsync(new CreatePlayerPacket { Origin = Origin.Server, PlayerId = player.Id }, ct);
            }

            await SendServerMessageAsync($"Player {player.Name} joined the game!", ct);
        }

        public async Task Handle(PlayerLeavedNotification notification, CancellationToken ct)
        {
            var (id, name, _) = notification;

            if (Id == id) return;
            await SendPacketAsync(new DestroyPlayerPacket { Origin = Origin.Server, PlayerId = id }, ct);
            await SendServerMessageAsync($"Player {name} leaved the game!", ct);
        }

        public async Task Handle(PlayerUpdatedStateNotification notification, CancellationToken ct)
        {
            var player = notification.Player;

            if (player.Features.Get<IP3DPlayerState>() is { } state)
                await SendPacketAsync(GetFromP3DPlayerState(player, state), ct);
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            var (player, message) = notification;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Id, Message = message }, ct);
        }

        public async Task Handle(PlayerSentLocalMessageNotification notification, CancellationToken ct)
        {
            var (player, location, message) = notification;

            if (LevelFile.Equals(location, StringComparison.OrdinalIgnoreCase))
                await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Id, Message = message }, ct);
        }

        public async Task Handle(PlayerSentPrivateMessageNotification notification, CancellationToken ct)
        {
            var (player, receiverName, message) = notification;

            if (Name.Equals(receiverName, StringComparison.OrdinalIgnoreCase))
                await SendPacketAsync(new ChatMessagePrivatePacket { Origin = player.Id, DestinationPlayerName = receiverName, Message = message }, ct);
        }

        public async Task Handle(MessageToPlayerNotification notification, CancellationToken ct)
        {
            var (from, to, message) = notification;

            if (Id != to.Id) return;
            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = from.Id, Message = message }, ct);
        }

        public async Task Handle(PlayerSentRawP3DPacketNotification notification, CancellationToken ct)
        {
            var player = notification.Player;

            if (Id == player.Id) return;
            await SendPacketAsync(notification.Packet, ct);
        }

        public async Task Handle(ServerMessageNotification notification, CancellationToken ct)
        {
            await SendServerMessageAsync(notification.Message, ct);
        }

    }
}