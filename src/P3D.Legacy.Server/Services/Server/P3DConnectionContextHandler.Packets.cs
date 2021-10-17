using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets;
using P3D.Legacy.Common.Packets.Battle;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Shared;
using P3D.Legacy.Common.Packets.Trade;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Notifications;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Server
{
    public partial class P3DConnectionContextHandler
    {
        private async Task HandlePacketAsync(P3DPacket? packet, CancellationToken ct)
        {
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
                case BattleOfferPacket battleOfferPacket:
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
                case TradeOfferPacket tradeOfferPacket:
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
            }
        }

        private void ParseGameData(GameDataPacket packet)
        {
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
                        GameMode = packet.GameMode;
                        break;

                    case 1:
                        IsGameJoltPlayer = packet.IsGameJoltPlayer;
                        break;

                    case 2:
                        GameJoltId = packet.GameJoltId;
                        break;

                    case 3:
                        DecimalSeparator = packet.DecimalSeparator;
                        break;

                    case 4:
                        Name = packet.Name;
                        break;

                    case 5:
                        LevelFile = packet.LevelFile;
                        break;

                    case 6:
                        Position = packet.Position;
                        break;

                    case 7:
                        Facing = packet.Facing;
                        break;

                    case 8:
                        Moving = packet.Moving;
                        break;

                    case 9:
                        Skin = packet.Skin;
                        break;

                    case 10:
                        BusyType = packet.BusyType;
                        //Basic.ServersManager.UpdatePlayerList();
                        break;

                    case 11:
                        MonsterVisible = packet.MonsterVisible;
                        break;

                    case 12:
                        MonsterPosition = packet.MonsterPosition;
                        break;

                    case 13:
                        MonsterSkin = packet.MonsterSkin;
                        break;

                    case 14:
                        MonsterFacing = packet.MonsterFacing;
                        break;
                }
            }
        }

        private async Task HandleGameDataAsync(GameDataPacket packet, CancellationToken ct)
        {
            if (_connectionState == P3DConnectionState.None)
            {
                _connectionState = P3DConnectionState.Initializing;
                await _mediator.Send(new PlayerInitializingCommand(this), ct);
            }

            ParseGameData(packet);
            await _mediator.Publish(new PlayerUpdatedStateNotification(this), ct);

            if (_connectionState != P3DConnectionState.Intitialized)
            {
                _connectionState = P3DConnectionState.Intitialized;
                await _mediator.Send(new PlayerReadyCommand(this), ct);

                await SendPacketAsync(new IdPacket
                {
                    Origin = Origin.Server,
                    PlayerId = Id

                }, ct);
                await SendPacketAsync(new WorldDataPacket
                {
                    Origin = Origin.Server,

                    Season = _worldService.Season,
                    Weather = _worldService.Weather,
                    CurrentTime = $"{_worldService.CurrentTime.Hours:00},{_worldService.CurrentTime.Minutes:00},{_worldService.CurrentTime.Seconds:00}"
                }, ct);

                await _mediator.Publish(new PlayerJoinedNotification(this), ct);
            }

            await SendPacketAsync(GetFromP3DPlayerState(this, this), ct);
        }

        private async Task HandleChatMessageAsync(ChatMessageGlobalPacket packet, CancellationToken ct)
        {
            var message = packet.Message;

            if (message.StartsWith("/"))
            {
                await _mediator.Publish(new PlayerSentCommandNotification(this, packet.Message), ct);
            }
            else
            {
                //await _mediator.Publish(new PlayerSentLocalMessageNotification(this, LevelFile, packet.Message), ct);
                await _mediator.Publish(new PlayerSentGlobalMessageNotification(this, packet.Message), ct);
            }
        }
        private async Task HandlePrivateMessageAsync(ChatMessagePrivatePacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentPrivateMessageNotification(this, packet.DestinationPlayerName, packet.Message), ct);
        }


        private async Task HandleGameStateMessageAsync(GameStateMessagePacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }

        private async Task HandleTradeRequestAsync(TradeRequestPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleTradeJoinAsync(TradeJoinPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleTradeQuitAsync(TradeQuitPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleTradeOfferAsync(TradeOfferPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleTradeStartAsync(TradeStartPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }

        private async Task HandleBattleClientDataAsync(BattleClientDataPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleHostDataAsync(BattleHostDataPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleJoinAsync(BattleJoinPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleOfferAsync(BattleOfferPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattlePokemonDataAsync(BattleEndRoundDataPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleQuitAsync(BattleQuitPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleRequestAsync(BattleRequestPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }
        private async Task HandleBattleStartAsync(BattleStartPacket packet, CancellationToken ct)
        {
            await _mediator.Publish(new PlayerSentRawP3DPacketNotification(this, packet), ct);
        }


        private async Task HandleServerDataRequestAsync(ServerDataRequestPacket packet, CancellationToken ct)
        {
            var clientNames = await _playerQueries.GetAllAsync(ct).Select(x => x.Name).ToArrayAsync(ct);
            await SendPacketAsync(new ServerInfoDataPacket
            {
                Origin = Origin.Server,

                CurrentPlayers = clientNames.Length,
                MaxPlayers = _serverOptions.MaxPlayers,
                PlayerNames = clientNames,

                ServerName = _serverOptions.ServerName,
                ServerMessage = _serverOptions.ServerMessage,
            }, ct);

            var lifetimeNotificationFeature = Features.Get<IConnectionLifetimeNotificationFeature>();
            lifetimeNotificationFeature.RequestClose();
        }
    }
}