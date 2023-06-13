using P3D.Legacy.Common;
using P3D.Legacy.Common.PlayerEvents;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Commands.Trade;
using P3D.Legacy.Server.Application.Queries.Options;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.Client.P3D.Events;
using P3D.Legacy.Server.Client.P3D.Packets.Battle;
using P3D.Legacy.Server.Client.P3D.Packets.Chat;
using P3D.Legacy.Server.Client.P3D.Packets.Server;
using P3D.Legacy.Server.Client.P3D.Packets.Trade;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class P3DConnectionContextHandler :
        IEventHandler<PlayerJoinedEvent>,
        IEventHandler<PlayerLeftEvent>,
        IEventHandler<PlayerUpdatedStateEvent>,
        //IEventHandler<PlayerUpdatedPositionEvent>,
        IEventHandler<PlayerSentGlobalMessageEvent>,
        IEventHandler<PlayerSentLocalMessageEvent>,
        IEventHandler<PlayerSentPrivateMessageEvent>,
        IEventHandler<MessageToPlayerEvent>,
        IEventHandler<PlayerSentRawP3DPacketEvent>,
        IEventHandler<ServerMessageEvent>,
        IEventHandler<PlayerTriggeredEventEvent>,
        IEventHandler<PlayerSentCommandEvent>,
        IEventHandler<WorldUpdatedEvent>,
        IEventHandler<PlayerSentLoginEvent>,
        IEventHandler<PlayerTradeInitiatedEvent>,
        IEventHandler<PlayerTradeAcceptedEvent>,
        IEventHandler<PlayerTradeAbortedEvent>,
        IEventHandler<PlayerTradeOfferedP3DMonsterEvent>,
        IEventHandler<PlayerTradeConfirmedEvent>,
        IEventHandler<ServerStoppingEvent>
    {
        public async Task HandleAsync(IReceiveContext<PlayerJoinedEvent> context, CancellationToken ct)
        {
            var player = context.Message.Player;

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

            await SendPacketAsync(new CreatePlayerPacket { Origin = Origin.Server, PlayerOrigin = player.Origin }, ct);

            await SendServerMessageAsync($"Player {player.Name} joined the server!", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerLeftEvent> context, CancellationToken ct)
        {
            var (_, origin, name) = context.Message;

            if (Origin == origin)
            {
                _finalizationDelayer.SetResult();
                return;
            }

            await SendPacketAsync(new DestroyPlayerPacket { Origin = Origin.Server, PlayerOrigin = origin }, ct);
            await SendServerMessageAsync($"Player {name} left the server!", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerUpdatedStateEvent> context, CancellationToken ct)
        {
            var player = context.Message.Player;

            if (player is IP3DPlayerState state)
            {
                await SendPacketAsync(GetFromP3DPlayerState(player, state), ct);
            }
        }

        /* We delegate it to PlayerUpdatedStateEvent. TODO: Do something better
        public async Task HandleAsync(IReceiveContext<PlayerUpdatedPositionEvent> context, CancellationToken ct)
        {
            var player = context.Message.Player;

            if (Id == player.Id) return;

            if (player is IP3DPlayerState state && state.LevelFile.Equals(LevelFile, StringComparison.Ordinal))
            {
                await SendPacketAsync(GetFromP3DPlayerState(player, state), ct);
            }
        }
        */

        public async Task HandleAsync(IReceiveContext<PlayerSentGlobalMessageEvent> context, CancellationToken ct)
        {
            var (player, message) = context.Message;

            if (Id != player.Id && await _queryDispatcher.DispatchAsync(new GetPlayerMuteStateQuery(Id, player.Id), ct)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Origin, Message = message }, ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentLocalMessageEvent> context, CancellationToken ct)
        {
            var (player, location, message) = context.Message;

            if (!LevelFile.Equals(location, StringComparison.OrdinalIgnoreCase)) return;
            if (Id != player.Id && await _queryDispatcher.DispatchAsync(new GetPlayerMuteStateQuery(Id, player.Id), ct)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Origin, Message = message }, ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentPrivateMessageEvent> context, CancellationToken ct)
        {
            var (player, receiverName, message) = context.Message;

            if (!Name.Equals(receiverName, StringComparison.OrdinalIgnoreCase)) return;
            if (Id != player.Id && await _queryDispatcher.DispatchAsync(new GetPlayerMuteStateQuery(Id, player.Id), ct)) return;

            await SendPacketAsync(new ChatMessagePrivateToClientPacket { Origin = player.Origin, DestinationPlayerName = receiverName, Message = message }, ct);
        }

        public async Task HandleAsync(IReceiveContext<MessageToPlayerEvent> context, CancellationToken ct)
        {
            var (from, to, message) = context.Message;

            if (Origin != to.Origin) return;
            if (Id != from.Id && await _queryDispatcher.DispatchAsync(new GetPlayerMuteStateQuery(Id, from.Id), ct)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = from.Origin, Message = message }, ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentRawP3DPacketEvent> context, CancellationToken ct)
        {
            var (player, p3dPacket) = context.Message;

            if (Origin == player.Origin) return;

            switch (p3dPacket)
            {
                case BattleRequestPacket when Id.GameJoltIdOrNone.IsNone != player.Id.GameJoltIdOrNone.IsNone:
                    await _eventDispatcher.DispatchAsync(new MessageToPlayerEvent(IPlayer.Server, player, "GameJolt and Non-GameJolt interaction is not supported!"), ct);
                    await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(player, new BattleQuitPacket { Origin = Origin, DestinationPlayerOrigin = player.Origin }), ct);
                    return;
                case BattleOfferFromClientPacket packet when packet.DestinationPlayerOrigin != Origin: return;
                case BattleOfferFromClientPacket packet:
                    await SendPacketAsync(new BattleOfferToClientPacket { Origin = packet.Origin, BattleData = packet.BattleData }, ct);
                    return;
                case BattleClientDataFromClientPacket packet when packet.DestinationPlayerOrigin != Origin: return;
                case BattleClientDataFromClientPacket packet:
                    await SendPacketAsync(new BattleClientDataToClientPacket { Origin = packet.Origin, BattleData = packet.BattleData }, ct);
                    return;
                case BattleHostDataFromClientPacket packet when packet.DestinationPlayerOrigin != Origin: return;
                case BattleHostDataFromClientPacket packet:
                    await SendPacketAsync(new BattleHostDataToClientPacket { Origin = packet.Origin, BattleData = packet.BattleData }, ct);
                    return;
                case BattleHostEndRoundDataFromClientPacket packet when packet.DestinationPlayerOrigin != Origin: return;
                case BattleHostEndRoundDataFromClientPacket packet:
                    await SendPacketAsync(new BattleHostEndRoundDataToClientPacket { Origin = packet.Origin, BattleData = packet.BattleData }, ct);
                    return;
                default:
                    await SendPacketAsync(p3dPacket, ct);
                    return;
            }
        }

        public async Task HandleAsync(IReceiveContext<ServerMessageEvent> context, CancellationToken ct)
        {
            var message = context.Message.Message;

            await SendServerMessageAsync(message, ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerTriggeredEventEvent> context, CancellationToken ct)
        {
            var (player, @event) = context.Message;

            await SendServerMessageAsync($"The player {player.Name} {PlayerEventParser.AsText(@event)}", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentCommandEvent> context, CancellationToken ct)
        {
            var (player, message) = context.Message;

            if (Origin != player.Origin) return;
            if (message.StartsWith("/login", StringComparison.OrdinalIgnoreCase)) return;

            await SendPacketAsync(new ChatMessageGlobalPacket { Origin = player.Origin, Message = message }, ct);
        }

        public async Task HandleAsync(IReceiveContext<WorldUpdatedEvent> context, CancellationToken ct)
        {
            var (time, season, weather) = context.Message.State;
            await SendPacketAsync(new WorldDataPacket
            {
                Origin = Origin.Server,

                Season = season,
                Weather = weather,
                CurrentTime = $"{time.Hours:00},{time.Minutes:00},{time.Seconds:00}",
            }, ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentLoginEvent> context, CancellationToken ct)
        {
            var (player, password) = context.Message;

            if (Origin != player.Origin) return;
            if (State != PlayerState.Authentication) return;

            if (await _commandDispatcher.DispatchAsync(new PlayerAuthenticateDefaultCommand(this, password), ct) is { IsSuccess: true })
            {
                await _commandDispatcher.DispatchAsync(new PlayerReadyCommand(this), ct);
                State = PlayerState.Initialized;
            }
        }

        public async Task HandleAsync(IReceiveContext<PlayerTradeInitiatedEvent> context, CancellationToken ct)
        {
            var (initiator, target) = context.Message;

            if (Origin != target) return;

            if (Id.GameJoltIdOrNone.IsNone != initiator.Id.GameJoltIdOrNone.IsNone)
            {
                await _eventDispatcher.DispatchAsync(new MessageToPlayerEvent(IPlayer.Server, initiator, "GameJolt and Non-GameJolt interaction is not supported!"), ct);
                await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(initiator, new TradeQuitPacket { Origin = target, DestinationPlayerOrigin = target }), ct);
                return;
            }

            if (await _commandDispatcher.DispatchAsync(new TradeOfferCommand(initiator, this), ct) is { IsSuccess: true })
            {
                await SendPacketAsync(new TradeRequestPacket { Origin = initiator.Origin, DestinationPlayerOrigin = target }, ct);
            }
            else
            {
                await _commandDispatcher.DispatchAsync(new TradeAbortCommand(initiator, this), ct);
                await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(initiator, new TradeQuitPacket { Origin = target, DestinationPlayerOrigin = target }), ct);
            }
        }

        public async Task HandleAsync(IReceiveContext<PlayerTradeAcceptedEvent> context, CancellationToken ct)
        {
            var (target, initiator) = context.Message;

            if (Origin != initiator) return;

            if (await _commandDispatcher.DispatchAsync(new TradeAcceptCommand(target, this), ct) is { IsSuccess: true })
            {
                await SendPacketAsync(new TradeJoinPacket { Origin = target.Origin, DestinationPlayerOrigin = initiator }, ct);
            }
            else
            {
                await _commandDispatcher.DispatchAsync(new TradeAbortCommand(target, this), ct);
                await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(target, new TradeQuitPacket { Origin = initiator, DestinationPlayerOrigin = target.Origin }), ct);
            }
        }

        public async Task HandleAsync(IReceiveContext<PlayerTradeAbortedEvent> context, CancellationToken ct)
        {
            var (player, partner) = context.Message;

            if (Origin != partner) return;

            await _commandDispatcher.DispatchAsync(new TradeAbortCommand(player, this), ct);
            await SendPacketAsync(new TradeQuitPacket { Origin = player.Origin, DestinationPlayerOrigin = partner }, ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerTradeOfferedP3DMonsterEvent> context, CancellationToken ct)
        {
            var (player, target, data) = context.Message;

            if (Origin != target) return;

            var cancel = false;
            var serverOptions = await _queryDispatcher.DispatchAsync(new GetServerOptionsQuery(), ct);
            if (serverOptions.ValidationEnabled)
            {
                var monster = await _p3dMonsterConverter.FromP3DStringAsync(data.MonsterData, ct);
                cancel = !await _monsterValidator.ValidateAsync(monster, ct);
            }

            if (cancel)
            {
                await _commandDispatcher.DispatchAsync(new TradeAbortCommand(player, this), ct);
                await _eventDispatcher.DispatchAsync(new MessageToPlayerEvent(IPlayer.Server, player, "The Pokemon is not valid!"), ct);
                await SendPacketAsync(new TradeQuitPacket { Origin = player.Origin, DestinationPlayerOrigin = target }, ct);
                await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, new TradeQuitPacket { Origin = target, DestinationPlayerOrigin = player.Origin }), ct);
            }
            else
            {
                await SendPacketAsync(new TradeOfferToClientPacket { Origin = player.Origin, TradeData = data }, ct);
            }
        }

        public async Task HandleAsync(IReceiveContext<PlayerTradeConfirmedEvent> context, CancellationToken ct)
        {
            var (player, target) = context.Message;

            if (Origin != target) return;

            if (await _commandDispatcher.DispatchAsync(new TradeConfirmCommand(player, this), ct) is { IsSuccess: true })
                await SendPacketAsync(new TradeStartPacket { Origin = player.Origin, DestinationPlayerOrigin = target }, ct);
            else
                await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(player, new TradeQuitPacket { Origin = target, DestinationPlayerOrigin = player.Origin }), ct);
        }

        public async Task HandleAsync(IReceiveContext<ServerStoppingEvent> context, CancellationToken ct)
        {
            var reason = context.Message.Reason;

            await SendPacketAsync(new ServerClosePacket { Reason = reason }, CancellationToken.None);
        }
    }
}