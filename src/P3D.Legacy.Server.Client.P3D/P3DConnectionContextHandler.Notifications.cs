using MediatR;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Battle;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Trade;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    // ReSharper disable once ArrangeTypeModifiers
    partial class P3DConnectionContextHandler :
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>,
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

            if (Origin == player.Origin)
            {
                await foreach (var connectedPlayer in _playerContainer.GetAllAsync(ct))
                {
                    await SendPacketAsync(new CreatePlayerPacket { Origin = Origin.Server, PlayerOrigin = connectedPlayer.Origin }, ct);
                    var state = connectedPlayer as IP3DPlayerState ?? IP3DPlayerState.Empty;
                    await SendPacketAsync(GetFromP3DPlayerState(connectedPlayer, state), ct);
                }
            }
            else
            {
                await SendPacketAsync(new CreatePlayerPacket { Origin = Origin.Server, PlayerOrigin = player.Origin }, ct);
            }

            await SendServerMessageAsync($"Player {player.Name} joined the server!", ct);
        }

        public async Task Handle(PlayerLeftNotification notification, CancellationToken ct)
        {
            var (_, origin, name) = notification;

            if (Origin == origin) return;

            await SendPacketAsync(new DestroyPlayerPacket { Origin = Origin.Server, PlayerOrigin = origin }, ct);
            await SendServerMessageAsync($"Player {name} left the server!", ct);
        }

        public async Task Handle(PlayerUpdatedStateNotification notification, CancellationToken ct)
        {
            var player = notification.Player;

            var state = player as IP3DPlayerState ?? IP3DPlayerState.Empty;
            await SendPacketAsync(GetFromP3DPlayerState(player, state), ct);
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            var (player, message) = notification;

            if (await _muteManager.IsMutedAsync(Id, player.Id, ct)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Origin, Message = message }, ct);
        }

        public async Task Handle(PlayerSentLocalMessageNotification notification, CancellationToken ct)
        {
            var (player, location, message) = notification;

            if (!LevelFile.Equals(location, StringComparison.OrdinalIgnoreCase)) return;
            if (await _muteManager.IsMutedAsync(Id, player.Id, ct)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Origin, Message = message }, ct);
        }

        public async Task Handle(PlayerSentPrivateMessageNotification notification, CancellationToken ct)
        {
            var (player, receiverName, message) = notification;

            if (!Name.Equals(receiverName, StringComparison.OrdinalIgnoreCase)) return;
            if (await _muteManager.IsMutedAsync(Id, player.Id, ct)) return;

            await SendPacketAsync(new ChatMessagePrivatePacket { Origin = player.Origin, DestinationPlayerName = receiverName, Message = message }, ct);
        }

        public async Task Handle(MessageToPlayerNotification notification, CancellationToken ct)
        {
            var (from, to, message) = notification;

            if (Origin != to.Origin) return;
            if (await _muteManager.IsMutedAsync(Id, from.Id, ct)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = from.Origin, Message = message }, ct);
        }

        public async Task Handle(PlayerSentRawP3DPacketNotification notification, CancellationToken ct)
        {
            var (player, p3dPacket) = notification;

            if (Origin == player.Origin) return;
            if (GameJoltId.IsNone != player.GameJoltId.IsNone)
            {
                switch (p3dPacket)
                {
                    case TradeRequestPacket:
                        await _notificationPublisher.Publish(new MessageToPlayerNotification(IPlayer.Server, player, "GameJolt and Non-GameJolt interaction is not supported!"), ct);
                        await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(player, new TradeQuitPacket { Origin = Origin, DestinationPlayerOrigin = player.Origin }), ct);
                        break;
                    case BattleRequestPacket:
                        await _notificationPublisher.Publish(new MessageToPlayerNotification(IPlayer.Server, player, "GameJolt and Non-GameJolt interaction is not supported!"), ct);
                        await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(player, new BattleQuitPacket { Origin = Origin, DestinationPlayerOrigin = player.Origin }), ct);
                        break;
                    case TradeQuitPacket:
                        await SendPacketAsync(p3dPacket, ct);
                        break;
                    case BattleQuitPacket:
                        await SendPacketAsync(p3dPacket, ct);
                        break;
                }
                return;
            }

            await SendPacketAsync(p3dPacket, ct);
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

            if (Origin != player.Origin) return;
            if (message.StartsWith("/login", StringComparison.OrdinalIgnoreCase)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Origin, Message = message }, ct);
        }

        public async Task Handle(WorldUpdatedNotification notification, CancellationToken ct)
        {
            var state = notification.State;
            await SendPacketAsync(new WorldDataPacket
            {
                Origin = Origin.Server,

                Season = state.Season,
                Weather = state.Weather,
                CurrentTime = $"{state.Time.Hours:00},{state.Time.Minutes:00},{state.Time.Seconds:00}"
            }, ct);
        }

        public async Task Handle(PlayerSentLoginNotification notification, CancellationToken ct)
        {
            var (player, password) = notification;

            if (Origin != player.Origin) return;
            if (_connectionState != P3DConnectionState.Authentication) return;

            if (await _mediator.Send(new PlayerAuthenticateDefaultCommand(this, password), ct) is { Success: true })
            {
                await _mediator.Send(new PlayerReadyCommand(this), ct);
                _connectionState = P3DConnectionState.Intitialized;
            }
        }
    }
}