using MediatR;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Shared;
using P3D.Legacy.Server.Models;
using P3D.Legacy.Server.Notifications;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Server
{
    public partial class P3DConnectionContextHandler :
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeavedNotification>,
        INotificationHandler<PlayerSentGameDataNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<PlayerSentLocalMessageNotification>,
        INotificationHandler<PlayerSentPrivateMessageNotification>
    {
        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            var (id, name, _) = notification;

            if (Id == id)
            {
                await foreach (var player in _playerContainer.GetAllAsync(ct))
                {
                    if (player.Features.Get<IP3DPlayerState>() is { } state)
                    {
                        await SendPacketAsync(new CreatePlayerPacket { Origin = Origin.Server, PlayerId = player.Id }, ct);
                        await SendPacketAsync(GetFromP3DPlayerState(player, state), ct);
                    }
                }
            }
            else
            {
                await SendPacketAsync(new CreatePlayerPacket { Origin = Origin.Server, PlayerId = id }, ct);
            }

            await SendServerMessageAsync($"Player {name} joined the game!", ct);
        }

        public async Task Handle(PlayerLeavedNotification notification, CancellationToken ct)
        {
            var (id, name, _) = notification;
            if (Id == id) return;
            await SendPacketAsync(new DestroyPlayerPacket { Origin = Origin.Server, PlayerId = id }, ct);
            await SendServerMessageAsync($"Player {name} leaved the game!", ct);
        }

        public async Task Handle(PlayerSentGameDataNotification notification, CancellationToken ct)
        {
            var (id, _, _, dataItemStorage) = notification;

            await SendPacketAsync(new GameDataPacket { Origin = id, DataItemStorage = { dataItemStorage } }, ct);
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            var (id, _, _, message) = notification;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = id, Message = message }, ct);
        }

        public async Task Handle(PlayerSentLocalMessageNotification notification, CancellationToken ct)
        {
            var (id, _, _, location, message) = notification;

            if (LevelFile.Equals(location, StringComparison.OrdinalIgnoreCase))
                await SendPacketAsync(new ChatMessageGlobalPacket { Origin = id, Message = message }, ct);
        }

        public async Task Handle(PlayerSentPrivateMessageNotification notification, CancellationToken ct)
        {
            var (id, _, _, receiverName, message) = notification;

            if (Name.Equals(receiverName, StringComparison.OrdinalIgnoreCase))
                await SendPacketAsync(new ChatMessagePrivatePacket { Origin = id, DestinationPlayerName = receiverName, Message = message }, ct);
        }
    }
}
