﻿using P3D.Legacy.Common;
using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Common.Packets.Battle;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Trade;
using P3D.Legacy.Common.PlayerEvents;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Commands.Trade;
using P3D.Legacy.Server.Application.Queries.Options;
using P3D.Legacy.Server.Application.Queries.Player;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    // ReSharper disable once ArrangeTypeModifiers
    [SuppressMessage("Performance", "CA1812")]
    internal partial class P3DConnectionContextHandler :
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>,
        INotificationHandler<PlayerUpdatedStateNotification>,
        INotificationHandler<PlayerUpdatedPositionNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<PlayerSentLocalMessageNotification>,
        INotificationHandler<PlayerSentPrivateMessageNotification>,
        INotificationHandler<MessageToPlayerNotification>,
        INotificationHandler<PlayerSentRawP3DPacketNotification>,
        INotificationHandler<ServerMessageNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>,
        INotificationHandler<PlayerSentCommandNotification>,
        INotificationHandler<WorldUpdatedNotification>,
        INotificationHandler<PlayerSentLoginNotification>,
        INotificationHandler<PlayerTradeInitiatedNotification>,
        INotificationHandler<PlayerTradeAcceptedNotification>,
        INotificationHandler<PlayerTradeAbortedNotification>,
        INotificationHandler<PlayerTradeOfferedPokemonNotification>,
        INotificationHandler<PlayerTradeConfirmedNotification>
    {
        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            var player = notification.Player;

            if (Origin == player.Origin)
            {
                var players = await _queryDispatcher.DispatchAsync(new GetPlayersInitializedQuery(), ct);
                foreach (var connectedPlayer in players)
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

            if (player is IP3DPlayerState state)
            {
                await SendPacketAsync(GetFromP3DPlayerState(player, state), ct);
            }
        }

        public async Task Handle(PlayerUpdatedPositionNotification notification, CancellationToken ct)
        {
            var player = notification.Player;

            if (player is IP3DPlayerState state && state.LevelFile.Equals(LevelFile, StringComparison.Ordinal))
            {
                await SendPacketAsync(GetFromP3DPlayerState(player, state), ct);
            }
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            var (player, message) = notification;

            if (Id != player.Id && await _queryDispatcher.DispatchAsync(new GetPlayerMuteStateQuery(Id, player.Id), ct)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Origin, Message = message }, ct);
        }

        public async Task Handle(PlayerSentLocalMessageNotification notification, CancellationToken ct)
        {
            var (player, location, message) = notification;

            if (!LevelFile.Equals(location, StringComparison.OrdinalIgnoreCase)) return;
            if (Id != player.Id && await _queryDispatcher.DispatchAsync(new GetPlayerMuteStateQuery(Id, player.Id), ct)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Origin, Message = message }, ct);
        }

        public async Task Handle(PlayerSentPrivateMessageNotification notification, CancellationToken ct)
        {
            var (player, receiverName, message) = notification;

            if (!Name.Equals(receiverName, StringComparison.OrdinalIgnoreCase)) return;
            if (Id != player.Id && await _queryDispatcher.DispatchAsync(new GetPlayerMuteStateQuery(Id, player.Id), ct)) return;

            await SendPacketAsync(new ChatMessagePrivatePacket { Origin = player.Origin, DestinationPlayerName = receiverName, Message = message }, ct);
        }

        public async Task Handle(MessageToPlayerNotification notification, CancellationToken ct)
        {
            var (from, to, message) = notification;

            if (Origin != to.Origin) return;
            if (Id != from.Id && await _queryDispatcher.DispatchAsync(new GetPlayerMuteStateQuery(Id, from.Id), ct)) return;

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
                    case BattleRequestPacket:
                        await _notificationDispatcher.DispatchAsync(new MessageToPlayerNotification(IPlayer.Server, player, "GameJolt and Non-GameJolt interaction is not supported!"), ct);
                        await _notificationDispatcher.DispatchAsync(new PlayerSentRawP3DPacketNotification(player, new BattleQuitPacket { Origin = Origin, DestinationPlayerOrigin = player.Origin }), ct);
                        break;
                    case BattleQuitPacket:
                        await SendPacketAsync(p3dPacket, ct);
                        break;
                }
                return;
            }

            if (p3dPacket is BattleOfferFromClientPacket battleOfferFromClientPacket)
            {
                await SendPacketAsync(new BattleOfferToClientPacket { Origin = battleOfferFromClientPacket.Origin, BattleData = battleOfferFromClientPacket.BattleData }, ct);
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
            var (player, @event) = notification;

            await SendServerMessageAsync($"The player {player.Name} {PlayerEventParser.AsText(@event)}", ct);
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
            var (time, season, weather) = notification.State;
            await SendPacketAsync(new WorldDataPacket
            {
                Origin = Origin.Server,

                Season = season,
                Weather = weather,
                CurrentTime = $"{time.Hours:00},{time.Minutes:00},{time.Seconds:00}"
            }, ct);
        }

        public async Task Handle(PlayerSentLoginNotification notification, CancellationToken ct)
        {
            var (player, password) = notification;

            if (Origin != player.Origin) return;
            if (State != PlayerState.Authentication) return;

            if (await _commandDispatcher.DispatchAsync(new PlayerAuthenticateDefaultCommand(this, password), ct) is { IsSuccess: true })
            {
                await _commandDispatcher.DispatchAsync(new PlayerReadyCommand(this), ct);
                State = PlayerState.Initialized;
            }
        }

        public async Task Handle(PlayerTradeInitiatedNotification notification, CancellationToken ct)
        {
            var (initiator, target) = notification;

            if (Origin != target) return;

            if (GameJoltId.IsNone != initiator.GameJoltId.IsNone)
            {
                await _notificationDispatcher.DispatchAsync(new MessageToPlayerNotification(IPlayer.Server, initiator, "GameJolt and Non-GameJolt interaction is not supported!"), ct);
                await _notificationDispatcher.DispatchAsync(new PlayerSentRawP3DPacketNotification(initiator, new TradeQuitPacket { Origin = target, DestinationPlayerOrigin = target }), ct);
            }
            else
            {
                if (await _commandDispatcher.DispatchAsync(new TradeOfferCommand(initiator, this), ct) is { IsSuccess: true })
                {
                    await SendPacketAsync(new TradeRequestPacket { Origin = initiator.Origin, DestinationPlayerOrigin = target }, ct);
                }
                else
                {
                    await _commandDispatcher.DispatchAsync(new TradeAbortCommand(initiator, this), ct);
                    await _notificationDispatcher.DispatchAsync(new PlayerSentRawP3DPacketNotification(initiator, new TradeQuitPacket { Origin = target, DestinationPlayerOrigin = target }), ct);
                }
            }
        }

        public async Task Handle(PlayerTradeAcceptedNotification notification, CancellationToken ct)
        {
            var (target, initiator) = notification;

            if (Origin != initiator) return;

            if (await _commandDispatcher.DispatchAsync(new TradeAcceptCommand(target, this), ct) is { IsSuccess: true })
            {
                await SendPacketAsync(new TradeJoinPacket { Origin = target.Origin, DestinationPlayerOrigin = initiator }, ct);
            }
            else
            {
                await _commandDispatcher.DispatchAsync(new TradeAbortCommand(target, this), ct);
                await _notificationDispatcher.DispatchAsync(new PlayerSentRawP3DPacketNotification(target, new TradeQuitPacket { Origin = initiator, DestinationPlayerOrigin = target.Origin }), ct);
            }
        }

        public async Task Handle(PlayerTradeAbortedNotification notification, CancellationToken ct)
        {
            var (player, partner) = notification;

            if (Origin != partner) return;

            await _commandDispatcher.DispatchAsync(new TradeAbortCommand(player, this), ct);
            await SendPacketAsync(new TradeQuitPacket { Origin = player.Origin, DestinationPlayerOrigin = partner }, ct);
        }

        public async Task Handle(PlayerTradeOfferedPokemonNotification notification, CancellationToken ct)
        {
            var (player, target, data) = notification;

            if (Origin != target) return;

            var cancel = false;
            var serverOptions = await _queryDispatcher.DispatchAsync(new GetServerOptionsQuery(), ct);
            if (serverOptions.ValidationEnabled)
            {
                var monster = await _queryDispatcher.DispatchAsync(new GetMonsterByDataQuery(data.MonsterData), ct);
                cancel = !monster.IsValidP3D();
            }

            if (cancel)
            {
                await _commandDispatcher.DispatchAsync(new TradeAbortCommand(player, this), ct);
                await _notificationDispatcher.DispatchAsync(new MessageToPlayerNotification(IPlayer.Server, player, "The Pokemon is not valid!"), ct);
                await SendPacketAsync(new TradeQuitPacket { Origin = player.Origin, DestinationPlayerOrigin = target }, ct);
                await _notificationDispatcher.DispatchAsync(new PlayerSentRawP3DPacketNotification(this, new TradeQuitPacket { Origin = target, DestinationPlayerOrigin = player.Origin }), ct);
            }
            else
            {
                await SendPacketAsync(new TradeOfferToClientPacket { Origin = player.Origin, TradeData = data }, ct);
            }
        }

        public async Task Handle(PlayerTradeConfirmedNotification notification, CancellationToken ct)
        {
            var (player, target) = notification;

            if (Origin != target) return;

            if (await _commandDispatcher.DispatchAsync(new TradeConfirmCommand(player, this), ct) is { IsSuccess: true })
                await SendPacketAsync(new TradeStartPacket { Origin = player.Origin, DestinationPlayerOrigin = target }, ct);
            else
                await _notificationDispatcher.DispatchAsync(new PlayerSentRawP3DPacketNotification(player, new TradeQuitPacket { Origin = target, DestinationPlayerOrigin = player.Origin }), ct);
        }
    }
}