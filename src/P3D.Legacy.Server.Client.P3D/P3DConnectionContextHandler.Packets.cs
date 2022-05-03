using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Common.Packets;
using P3D.Legacy.Common.Packets.Battle;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Common;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Trade;
using P3D.Legacy.Common.PlayerEvents;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Queries.Options;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.Application.Queries.World;
using P3D.Legacy.Server.CQERS.Extensions;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class P3DConnectionContextHandler
    {
        private static readonly Action<ILogger, Exception?> DataItemsIsEmpty = LoggerMessage.Define(
            LogLevel.Warning, default, "P3D Reading Error: ParseGameData DataItems is empty");

        private static readonly Action<ILogger, string, Exception?> DataItemsCountLessThan14 = LoggerMessage.Define<string>(
            LogLevel.Warning, default, "P3D Reading Error: ParseGameData DataItems < 14. Packet DataItems {DataItems}");


        private async Task HandlePacketAsync(P3DPacket? packet, CancellationToken ct)
        {
            if (packet is null) return;

            using var span = _tracer.StartActiveSpan($"P3D Client Handle {packet.GetType().Name}");
            span.SetAttribute("p3dclient.packet_type", packet.GetType().FullName);

            switch (packet)
            {
                case BattleClientDataPacket battleClientDataPacket:
                    await HandleBattleClientDataAsync(battleClientDataPacket, ct);
                    break;
                case BattleHostDataPacket battleHostDataPacket:
                    await HandleBattleHostDataAsync(battleHostDataPacket, ct);
                    break;
                case BattleJoinPacket battleJoinPacket:
                    await HandleBattleJoinAsync(battleJoinPacket, ct);
                    break;
                case BattleOfferFromClientPacket battleOfferPacket:
                    await HandleBattleOfferAsync(battleOfferPacket, ct);
                    break;
                case BattleEndRoundDataPacket battleEndRoundDataPacket:
                    await HandleBattlePokemonDataAsync(battleEndRoundDataPacket, ct);
                    break;
                case BattleQuitPacket battleQuitPacket:
                    await HandleBattleQuitAsync(battleQuitPacket, ct);
                    break;
                case BattleRequestPacket battleRequestPacket:
                    await HandleBattleRequestAsync(battleRequestPacket, ct);
                    break;
                case BattleStartPacket battleStartPacket:
                    await HandleBattleStartAsync(battleStartPacket, ct);
                    break;
                case ChatMessageGlobalPacket chatMessageGlobalPacket:
                    await HandleChatMessageAsync(chatMessageGlobalPacket, ct);
                    break;
                case ChatMessagePrivatePacket chatMessagePrivatePacket:
                    await HandlePrivateMessageAsync(chatMessagePrivatePacket, ct);
                    break;

                case ServerDataRequestPacket serverDataRequestPacket:
                    await HandleServerDataRequestAsync(serverDataRequestPacket, ct);
                    break;
                case GameDataPacket gameDataPacket:
                    await HandleGameDataAsync(gameDataPacket, ct);
                    break;
                case GameStateMessagePacket gameStateMessagePacket:
                    await HandleGameStateMessageAsync(gameStateMessagePacket, ct);
                    break;
                case TradeJoinPacket tradeJoinPacket:
                    await HandleTradeJoinAsync(tradeJoinPacket, ct);
                    break;
                case TradeOfferFromClientPacket tradeOfferPacket:
                    await HandleTradeOfferAsync(tradeOfferPacket, ct);
                    break;
                case TradeQuitPacket tradeQuitPacket:
                    await HandleTradeQuitAsync(tradeQuitPacket, ct);
                    break;
                case TradeRequestPacket tradeRequestPacket:
                    await HandleTradeRequestAsync(tradeRequestPacket, ct);
                    break;
                case TradeStartPacket tradeStartPacket:
                    await HandleTradeStartAsync(tradeStartPacket, ct);
                    break;
                default:
                    if (State != PlayerState.Initialized) return;
                    await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
                    break;
            }
        }

        private void ParseGameData(GameDataPacket packet, out bool stateUpdated, out bool positionUpdated)
        {
            stateUpdated = false;
            positionUpdated = false;

            if (packet.DataItemStorage.Count == 0)
            {
                DataItemsIsEmpty(_logger, null);
                return;
            }

            if (packet.DataItemStorage.Count < 14)
            {
                DataItemsCountLessThan14(_logger, packet.DataItemStorage.ToString(), null);
                return;
            }

            for (var idx = 0; idx < packet.DataItemStorage.Count; idx++)
            {
                var rawData = packet.DataItemStorage.Get(idx);
                if (string.IsNullOrEmpty(rawData))
                    continue;

                switch (idx)
                {
                    case 0:
                        if (!string.Equals(GameMode, packet.GameMode, StringComparison.Ordinal))
                        {
                            GameMode = packet.GameMode;
                            stateUpdated = true;
                        }
                        break;

                    case 1:
                        if (!IsGameJoltPlayer.Equals(packet.IsGameJoltPlayer))
                        {
                            IsGameJoltPlayer = packet.IsGameJoltPlayer;
                            stateUpdated = true;
                        }
                        break;

                    case 2:
                        if (!GameJoltId.Equals(packet.GameJoltId))
                        {
                            GameJoltId = GameJoltId.FromNumber(packet.GameJoltId);
                            stateUpdated = true;
                        }
                        break;

                    case 3:
                        if (packet.DecimalSeparator is { } decimalSeparator && !DecimalSeparator.Equals(decimalSeparator))
                        {
                            DecimalSeparator = decimalSeparator;
                            stateUpdated = true;
                        }
                        break;

                    case 4:
                        if (!string.Equals(Name, packet.Name, StringComparison.Ordinal))
                        {
                            Name = packet.Name;
                            stateUpdated = true;
                        }
                        break;

                    case 5:
                        if (!string.Equals(LevelFile, packet.LevelFile, StringComparison.Ordinal))
                        {
                            LevelFile = packet.LevelFile;
                            positionUpdated = true;
                        }
                        break;

                    case 6:
                        if (!Position.Equals(packet.Position))
                        {
                            Position = packet.Position;
                            positionUpdated = true;
                        }
                        break;

                    case 7:
                        if (!Facing.Equals(packet.Facing))
                        {
                            Facing = packet.Facing;
                            positionUpdated = true;
                        }
                        break;

                    case 8:
                        if (!Moving.Equals(packet.Moving))
                        {
                            Moving = packet.Moving;
                            positionUpdated = true;
                        }
                        break;

                    case 9:
                        if (!string.Equals(Skin, packet.Skin, StringComparison.Ordinal))
                        {
                            Skin = packet.Skin;
                            stateUpdated = true;
                        }
                        break;

                    case 10:
                        if (!string.Equals(BusyType, packet.BusyType, StringComparison.Ordinal))
                        {
                            BusyType = packet.BusyType;
                            stateUpdated = true;
                        }
                        //Basic.ServersManager.UpdatePlayerList();
                        break;

                    case 11:
                        if (!MonsterVisible.Equals(packet.MonsterVisible))
                        {
                            MonsterVisible = packet.MonsterVisible;
                            positionUpdated = true;
                        }
                        break;

                    case 12:
                        if (!MonsterPosition.Equals(packet.MonsterPosition))
                        {
                            MonsterPosition = packet.MonsterPosition;
                            positionUpdated = true;
                        }
                        break;

                    case 13:
                        if (!string.Equals(MonsterSkin, packet.MonsterSkin, StringComparison.Ordinal))
                        {
                            MonsterSkin = packet.MonsterSkin;
                            stateUpdated = true;
                        }
                        break;

                    case 14:
                        if (!MonsterFacing.Equals(packet.MonsterFacing))
                        {
                            MonsterFacing = packet.MonsterFacing;
                            positionUpdated = true;
                        }
                        break;
                }
            }
        }

        private async Task HandleGameDataAsync(GameDataPacket packet, CancellationToken ct)
        {
            if (packet.DecimalSeparator is null)
                packet = packet with { DecimalSeparator = DecimalSeparator };

            ParseGameData(packet, out var stateUpdated, out var positionUpdated);

            if (State == PlayerState.None)
            {
                _connectionSpan.UpdateName($"P3D Session {Name}");
                _connectionSpan.SetAttribute("enduser.id", Name);

                State = PlayerState.Initializing;
                await _commandDispatcher.DispatchAsync(new PlayerInitializingCommand(this), ct);
            }

            if (State == PlayerState.Initializing)
            {
                var serverOptions = await _queryDispatcher.DispatchAsync(new GetServerOptionsQuery(), ct);
                var players = await _queryDispatcher.DispatchAsync(new GetPlayersInitializedQuery(), ct);

                if (players.Length >= serverOptions.MaxPlayers)
                {
                    await KickAsync("Server is full!", ct);
                    return;
                }

                if (players.Any(x => x.Id == Id))
                {
                    await KickAsync("You are already on the server!", ct);
                    return;
                }


                await SendPacketAsync(new IdPacket
                {
                    Origin = Origin.Server,
                    PlayerOrigin = Origin

                }, ct);
                var worldState = await _queryDispatcher.DispatchAsync(new GetWorldStateQuery(), ct);
                await SendPacketAsync(new WorldDataPacket
                {
                    Origin = Origin.Server,

                    Season = worldState.Season,
                    Weather = worldState.Weather,
                    CurrentTime = $"{worldState.Time.Hours:00},{worldState.Time.Minutes:00},{worldState.Time.Seconds:00}"
                }, ct);

                State = PlayerState.Authentication;

                switch (Id.IdType)
                {
                    case PlayerIdType.Name:
                        if (serverOptions.OfflineEnabled)
                        {
                            await SendServerMessageAsync("Please use /login %PASSWORD% for logging in or registering.", ct);
                            await SendServerMessageAsync("Please note that chat messages are not sent secure to the server.", ct);
                            await SendServerMessageAsync("The game client sends a SHA-512 hash instead of the raw password.", ct);
                            await SendServerMessageAsync("Don't use your regular passwords!", ct);
                        }
                        else
                        {
                            Id = PlayerId.None;
                            await KickAsync("Offline saves are not supported!", ct);
                            return;
                        }
                        break;
                    case PlayerIdType.GameJolt:
                        if (await _commandDispatcher.DispatchAsync(new PlayerAuthenticateGameJoltCommand(this, GameJoltId), ct) is { IsSuccess: true })
                        {
                            await _commandDispatcher.DispatchAsync(new PlayerReadyCommand(this), ct);
                            State = PlayerState.Initialized;
                        }
                        else
                        {
                            Id = PlayerId.None;
                            await KickAsync("Failed to verify your GameJolt Id!", ct);
                            return;
                        }
                        break;
                }
            }

            if (State == PlayerState.Initialized)
            {
                if (stateUpdated)
                {
                    await _eventDispatcher.DispatchAsync(new PlayerUpdatedStateEvent(this), ct);
                }
                if (positionUpdated)
                {
                    //await _eventDispatcher.Dispatch(new PlayerUpdatedPositionEvent(this), ct);
                }
            }

            await SendPacketAsync(GetFromP3DPlayerState(this, this), ct);
        }

        private async Task HandleChatMessageAsync(ChatMessageGlobalPacket packet, CancellationToken ct)
        {
            var message = packet.Message;

            if (message.StartsWith("/", StringComparison.Ordinal))
            {
                await _eventDispatcher.DispatchAsync(new PlayerSentCommandEvent(this, packet.Message), ct);
            }
            else if (State == PlayerState.Initialized)
            {
                //await _eventDispatcher.Dispatch(new PlayerSentLocalMessageEvent(this, LevelFile, packet.Message), ct);
                await _eventDispatcher.DispatchAsync(new PlayerSentGlobalMessageEvent(this, packet.Message), ct);
            }
        }
        private async Task HandlePrivateMessageAsync(ChatMessagePrivatePacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerSentPrivateMessageEvent(this, packet.DestinationPlayerName, packet.Message), ct);
        }


        private async Task HandleGameStateMessageAsync(GameStateMessagePacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            if (!PlayerEventParser.TryParse(packet.EventMessage, out var @event))
                return;

            await _eventDispatcher.DispatchAsync(new PlayerTriggeredEventEvent(this, @event), ct);
        }

        // 1. TradeRequestPacket
        // 2. TradeJoinPacket
        // 1. TradeOfferPacket
        // 2. TradeOfferPacket

        private async Task HandleTradeRequestAsync(TradeRequestPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerTradeInitiatedEvent(this, packet.DestinationPlayerOrigin), ct);
        }
        private async Task HandleTradeJoinAsync(TradeJoinPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerTradeAcceptedEvent(this, packet.DestinationPlayerOrigin), ct);
        }
        private async Task HandleTradeQuitAsync(TradeQuitPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerTradeAbortedEvent(this, packet.DestinationPlayerOrigin), ct);
        }
        private async Task HandleTradeOfferAsync(TradeOfferFromClientPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerTradeOfferedPokemonEvent(this, packet.DestinationPlayerOrigin, packet.TradeData), ct);
        }
        private async Task HandleTradeStartAsync(TradeStartPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerTradeConfirmedEvent(this, packet.DestinationPlayerOrigin), ct);
        }

        private async Task HandleBattleClientDataAsync(BattleClientDataPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
        }
        private async Task HandleBattleHostDataAsync(BattleHostDataPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
        }
        private async Task HandleBattleJoinAsync(BattleJoinPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
        }
        private async Task HandleBattleOfferAsync(BattleOfferFromClientPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            var cancel = false;
            var serverOptions = await _queryDispatcher.DispatchAsync(new GetServerOptionsQuery(), ct);
            if (serverOptions.ValidationEnabled)
            {
                var query = packet.BattleData.MonsterDatas
                    .ToAsyncEnumerable()
                    .SelectAwait(async x => await _queryDispatcher.DispatchAsync(new GetMonsterByDataQuery(x), ct))
                    .WithCancellation(ct);
                await foreach (var monster in query)
                {
                    if (!monster.IsValidP3D())
                    {
                        cancel = true;
                        break;
                    }
                }
            }

            if (cancel)
            {
                await SendServerMessageAsync("One of your Pokemon is not valid!", ct);
                await SendPacketAsync(new BattleQuitPacket { Origin = packet.DestinationPlayerOrigin, DestinationPlayerOrigin = Origin }, ct);
                await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, new BattleQuitPacket { Origin = Origin, DestinationPlayerOrigin = packet.DestinationPlayerOrigin }), ct);
            }
            else
            {
                await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
            }
        }
        private async Task HandleBattlePokemonDataAsync(BattleEndRoundDataPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
        }
        private async Task HandleBattleQuitAsync(BattleQuitPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
        }
        private async Task HandleBattleRequestAsync(BattleRequestPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
        }
        private async Task HandleBattleStartAsync(BattleStartPacket packet, CancellationToken ct)
        {
            if (State != PlayerState.Initialized)
                return;

            await _eventDispatcher.DispatchAsync(new PlayerSentRawP3DPacketEvent(this, packet), ct);
        }


        // ReSharper disable once UnusedParameter.Local
        private async Task HandleServerDataRequestAsync(ServerDataRequestPacket _, CancellationToken ct)
        {
            var serverOptions = await _queryDispatcher.DispatchAsync(new GetServerOptionsQuery(), ct);

            var players = await _queryDispatcher.DispatchAsync(new GetPlayersInitializedQuery(), ct);
            var clientNames = players.Select(x => x.Name).ToImmutableArray();

            await SendPacketAsync(new ServerInfoDataPacket
            {
                Origin = Origin.Server,

                CurrentPlayers = clientNames.Length,
                MaxPlayers = serverOptions.MaxPlayers,
                ServerName = serverOptions.Name,
                ServerMessage = serverOptions.Message,
                PlayerNames = clientNames,
            }, ct);

            var lifetimeNotificationFeature = Connection.Features.Get<IConnectionLifetimeNotificationFeature>();
            lifetimeNotificationFeature?.RequestClose();
        }
    }
}