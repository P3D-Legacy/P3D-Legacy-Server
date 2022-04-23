﻿using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Events;
using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Common.Packets;
using P3D.Legacy.Common.Packets.Battle;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Shared;
using P3D.Legacy.Common.Packets.Trade;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    // ReSharper disable once ArrangeTypeModifiers
    partial class P3DConnectionContextHandler
    {
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
                    if (_connectionState != P3DConnectionState.Intitialized) return;
                    await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
                    break;
            }
        }

        private void ParseGameData(GameDataPacket packet, out bool stateUpdated, out bool positionUpdated)
        {
            stateUpdated = false;
            positionUpdated = false;

            if (packet.DataItemStorage.Count == 0)
            {
                _logger.LogWarning("P3D Reading Error: ParseGameData DataItems is empty");
                return;
            }

            if (packet.DataItemStorage.Count < 14)
            {
                _logger.LogWarning("P3D Reading Error: ParseGameData DataItems < 14. Packet DataItems {DataItems}", packet.DataItemStorage);
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
                        if (!GameJoltId.Equals( packet.GameJoltId))
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

            if (_connectionState == P3DConnectionState.None)
            {
                _connectionSpan.UpdateName($"P3D Session {Name}");
                _connectionSpan.SetAttribute("enduser.id", Name);

                _connectionState = P3DConnectionState.Initializing;
                await _mediator.Send(new PlayerInitializingCommand(this), ct);
            }

            if (_connectionState == P3DConnectionState.Initializing)
            {
                await SendPacketAsync(new IdPacket
                {
                    Origin = Origin.Server,
                    PlayerOrigin = Origin

                }, ct);
                await SendPacketAsync(new WorldDataPacket
                {
                    Origin = Origin.Server,

                    Season = _worldService.Season,
                    Weather = _worldService.Weather,
                    CurrentTime = $"{_worldService.CurrentTime.Hours:00},{_worldService.CurrentTime.Minutes:00},{_worldService.CurrentTime.Seconds:00}"
                }, ct);

                _connectionState = P3DConnectionState.Authentication;

                switch (Id.IdType)
                {
                    case PlayerIdType.Name:
                        var currentOptions = _serverOptions.CurrentValue;
                        if (currentOptions.OfflineEnabled)
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
                        if (await _mediator.Send(new PlayerAuthenticateGameJoltCommand(this, GameJoltId), ct) is { Success: true })
                        {
                            await _mediator.Send(new PlayerReadyCommand(this), ct);
                            _connectionState = P3DConnectionState.Intitialized;
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

            if (_connectionState == P3DConnectionState.Intitialized)
            {
                if (stateUpdated)
                {
                    await _notificationPublisher.Publish(new PlayerUpdatedStateNotification(this), ct);
                }
                if (positionUpdated)
                {
                    //await _notificationPublisher.Publish(new PlayerUpdatedPositionNotification(this), ct);
                }
            }

            await SendPacketAsync(GetFromP3DPlayerState(this, this), ct);
        }

        private async Task HandleChatMessageAsync(ChatMessageGlobalPacket packet, CancellationToken ct)
        {
            var message = packet.Message;

            if (message.StartsWith("/"))
            {
                await _notificationPublisher.Publish(new PlayerSentCommandNotification(this, packet.Message), ct);
            }
            else if (_connectionState == P3DConnectionState.Intitialized)
            {
                //await _notificationPublisher.Publish(new PlayerSentLocalMessageNotification(this, LevelFile, packet.Message), ct);
                await _notificationPublisher.Publish(new PlayerSentGlobalMessageNotification(this, packet.Message), ct);
            }
        }
        private async Task HandlePrivateMessageAsync(ChatMessagePrivatePacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerSentPrivateMessageNotification(this, packet.DestinationPlayerName, packet.Message), ct);
        }


        private async Task HandleGameStateMessageAsync(GameStateMessagePacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            if (!EventParser.TryParse(packet.EventMessage, out var @event))
                return;

            await _notificationPublisher.Publish(new PlayerTriggeredEventNotification(this, @event), ct);
        }

        // 1. TradeRequestPacket
        // 2. TradeJoinPacket
        // 1. TradeOfferPacket
        // 2. TradeOfferPacket

        private async Task HandleTradeRequestAsync(TradeRequestPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerTradeInitiatedNotification(this, packet.DestinationPlayerOrigin), ct);
        }
        private async Task HandleTradeJoinAsync(TradeJoinPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerTradeAcceptedNotification(this, packet.DestinationPlayerOrigin), ct);
        }
        private async Task HandleTradeQuitAsync(TradeQuitPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerTradeAbortedNotification(this, packet.DestinationPlayerOrigin), ct);
        }
        private async Task HandleTradeOfferAsync(TradeOfferFromClientPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerTradeOfferedPokemonNotification(this, packet.DestinationPlayerOrigin, packet.TradeData), ct);
        }
        private async Task HandleTradeStartAsync(TradeStartPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerTradeConfirmedNotification(this, packet.DestinationPlayerOrigin), ct);
        }

        private async Task HandleBattleClientDataAsync(BattleClientDataPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleHostDataAsync(BattleHostDataPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleJoinAsync(BattleJoinPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleOfferAsync(BattleOfferFromClientPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            var cancel = false;
            var currentServerOptions = _serverOptions.CurrentValue;
            if (currentServerOptions.ValidationEnabled)
            {
                await foreach (var monster in packet.BattleData.MonsterDatas.ToAsyncEnumerable().SelectAwait(async x => await _monsterRepository.GetByDataAsync(x)).WithCancellation(ct))
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
                await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, new BattleQuitPacket { Origin = Origin, DestinationPlayerOrigin = packet.DestinationPlayerOrigin }), ct);
            }
            else
            {
                await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
            }
        }
        private async Task HandleBattlePokemonDataAsync(BattleEndRoundDataPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleQuitAsync(BattleQuitPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleRequestAsync(BattleRequestPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleStartAsync(BattleStartPacket packet, CancellationToken ct)
        {
            if (_connectionState != P3DConnectionState.Intitialized)
                return;

            await _notificationPublisher.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }


        // ReSharper disable once UnusedParameter.Local
        private async Task HandleServerDataRequestAsync(ServerDataRequestPacket _, CancellationToken ct)
        {
            var currentServerOptions = _serverOptions.CurrentValue;

            var clientNames = await _playerContainer.GetAllAsync(ct).Where(x => x.Permissions > PermissionFlags.UnVerified).Select(x => x.Name).ToArrayAsync(ct);
            await SendPacketAsync(new ServerInfoDataPacket
            {
                Origin = Origin.Server,

                CurrentPlayers = clientNames.Length,
                MaxPlayers = currentServerOptions.MaxPlayers,
                ServerName = currentServerOptions.Name,
                ServerMessage = currentServerOptions.Message,
                PlayerNames = clientNames,
            }, ct);

            var lifetimeNotificationFeature = Connection.Features.Get<IConnectionLifetimeNotificationFeature>();
            lifetimeNotificationFeature?.RequestClose();
        }
    }
}