using MediatR;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    partial class P3DConnectionContextHandler :
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeavedNotification>,
        INotificationHandler<PlayerUpdatedStateNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<PlayerSentLocalMessageNotification>,
        INotificationHandler<PlayerSentPrivateMessageNotification>,
        INotificationHandler<MessageToPlayerNotification>,
        INotificationHandler<PlayerSentRawP3DPacketNotification>,
        INotificationHandler<ServerMessageNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>,
        INotificationHandler<PlayerSentCommandNotification>,
        INotificationHandler<WorldUpdatedNotification>,
        INotificationHandler<PlayerSentLoginNotification>
    {
        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            var player = notification.Player;

            if (Id == player.Id)
            {
                await foreach (var connectedPlayer in _playerContainer.GetAllAsync(ct))
                {
                    if (connectedPlayer is IP3DPlayerState state)
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

            await SendServerMessageAsync($"Player {player.Name} joined the server!", ct);
        }

        public async Task Handle(PlayerLeavedNotification notification, CancellationToken ct)
        {
            var (id, name, _) = notification;

            if (Id == id) return;

            await SendPacketAsync(new DestroyPlayerPacket { Origin = Origin.Server, PlayerId = id }, ct);
            await SendServerMessageAsync($"Player {name} left the server!", ct);
        }

        public async Task Handle(PlayerUpdatedStateNotification notification, CancellationToken ct)
        {
            var player = notification.Player;

            if (player is IP3DPlayerState state)
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

            if (!LevelFile.Equals(location, StringComparison.OrdinalIgnoreCase)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Id, Message = message }, ct);
        }

        public async Task Handle(PlayerSentPrivateMessageNotification notification, CancellationToken ct)
        {
            var (player, receiverName, message) = notification;

            if (!Name.Equals(receiverName, StringComparison.OrdinalIgnoreCase)) return;

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
            var (player, p3DPacket) = notification;

            if (Id == player.Id) return;
            if (GameJoltId.IsNone != player.GameJoltId.IsNone) return;

            await SendPacketAsync(p3DPacket, ct);
        }

        public async Task Handle(ServerMessageNotification notification, CancellationToken ct)
        {
            var message = notification.Message;

            await SendServerMessageAsync(message, ct);
        }

        public async Task Handle(PlayerTriggeredEventNotification notification, CancellationToken ct)
        {
            var (player, eventMessage) = notification;

            await SendServerMessageAsync($"The player {player.Name} {eventMessage}", ct);
        }

        public async Task Handle(PlayerSentCommandNotification notification, CancellationToken ct)
        {
            var (player, message) = notification;

            if (Id != player.Id) return;
            if (message.StartsWith("/login", StringComparison.OrdinalIgnoreCase)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Id, Message = message }, ct);
        }

        public async Task Handle(WorldUpdatedNotification notification, CancellationToken ct)
        {
            await SendPacketAsync(new WorldDataPacket
            {
                Origin = Origin.Server,

                Season = _worldService.Season,
                Weather = _worldService.Weather,
                CurrentTime = $"{_worldService.CurrentTime.Hours:00},{_worldService.CurrentTime.Minutes:00},{_worldService.CurrentTime.Seconds:00}"
            }, ct);
        }

        public async Task Handle(PlayerSentLoginNotification notification, CancellationToken ct)
        {
            var (player, password) = notification;

            if (Id != player.Id) return;
            if (_connectionState != P3DConnectionState.Authentication) return;

            if (await _mediator.Send(new PlayerAuthenticateDefaultCommand(this, password), ct) is { Success: true })
            {
                await _mediator.Send(new PlayerReadyCommand(this), ct);
                _connectionState = P3DConnectionState.Intitialized;
                await _mediator.Publish(new PlayerJoinedNotification(this), ct);
            }
        }
    }
}