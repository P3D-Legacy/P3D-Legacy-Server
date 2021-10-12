using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Shared;
using P3D.Legacy.Server.Services;

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public partial class P3DConnectionHandler
    {
        private bool IsOfficialGameMode =>
            string.Equals(GameMode, "Kolben", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(GameMode, "Pokemon3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(GameMode, "Pokémon3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(GameMode, "Pokemon 3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(GameMode, "Pokémon 3D", StringComparison.OrdinalIgnoreCase);

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

            GameMode = packet.GameMode;
            IsGameJoltPlayer = packet.IsGameJoltPlayer;
            GameJoltId = packet.GameJoltId;
            DecimalSeparator = packet.DecimalSeparator;

            Name = packet.Name;
            LevelFile = packet.LevelFile;

            Position = packet.Position;
            Facing = packet.Facing;
            Moving = packet.Moving;
            Skin = packet.Skin;
            BusyType = packet.BusyType;

            MonsterVisible = packet.MonsterVisible;
            MonsterPosition = packet.MonsterPosition;
            MonsterSkin = packet.MonsterSkin;
            MonsterFacing = packet.MonsterFacing;
        }

        private async Task HandleGameDataAsync(GameDataPacket packet, CancellationToken ct)
        {
            if (_connectionState == P3DConnectionState.None)
            {
                _connectionState = P3DConnectionState.Initializing;
                if (OnInitializingAsync is not null)
                    await OnInitializingAsync(_connectionIdFeature.ConnectionId, this);
            }

            ParseGameData(packet);

            //if (Moving)
            //    Module.OnPosition(this);
            //else
            //    Module.SendPacketToAll(packet);

            if (_connectionState != P3DConnectionState.Intitialized)
            {
                _playerHandlerService.OnEventAsync += PlayerHandlerService_OnEventAsync;

                _connectionState = P3DConnectionState.Intitialized;
                if (OnInitializedAsync is not null)
                    await OnInitializedAsync(_connectionIdFeature.ConnectionId, this);

                await _writer.WriteAsync(_protocol, new IdPacket
                {
                    Origin = Origin.Server,
                    PlayerId = Id

                }, ct);
                await _writer.WriteAsync(_protocol, new WorldDataPacket
                {
                    Origin = Origin.Server,

                    Season = _worldService.Season,
                    Weather = _worldService.Weather,
                    CurrentTime = $"{_worldService.CurrentTime.Hours:00},{_worldService.CurrentTime.Minutes:00},{_worldService.CurrentTime.Seconds:00}"
                }, ct);
            }

            await _writer.WriteAsync(_protocol, new GameDataPacket
            {
                Origin = Origin.Server,

                GameMode = GameMode,
                IsGameJoltPlayer = IsGameJoltPlayer,
                GameJoltId = GameJoltId,
                DecimalSeparator = DecimalSeparator,
                Name = Name,
                LevelFile = LevelFile,
                Position = Position,
                Facing = Facing,
                Moving = Moving,
                Skin = Skin,
                BusyType = BusyType,
                MonsterVisible = MonsterVisible,
                MonsterPosition = MonsterPosition,
                MonsterSkin = MonsterSkin,
                MonsterFacing = MonsterFacing
            }, ct);
        }

        private async Task PlayerHandlerService_OnEventAsync(Event arg)
        {
            switch (arg)
            {
                case PlayerJoinedEvent playerJoinedEvent:
                    await SendServerMessageAsync(playerJoinedEvent.Message);
                    break;
            }
        }

        private async Task HandleChatMessageAsync(ChatMessageGlobalPacket packet)
        {
            /*
            var message = new ChatMessage(this, packet.Message);
            if (packet.Message.StartsWith("/"))
            {
                // Do not show login command
                if (!packet.Message.ToLower().StartsWith("/login"))
                    SendChatMessage(null, message);

                if (Module.ExecuteClientCommand(this, packet.Message))
                    return;

                SendServerMessage("Invalid command!");
            }
            else if (IsInitialized)
            {
                Module.OnClientChatMessage(message);
            }
            */
        }
        private async Task HandlePrivateMessageAsync(ChatMessagePrivatePacket packet)
        {
            /*
            var destClient = Module.GetClient(packet.DestinationPlayerName);
            if (destClient != null)
            {
                await _writer.WriteAsync(_protocol, new ChatMessagePrivatePacket
                {
                    Origin = packet.Origin,
                    DataItems = packet.DataItems
                });
                destClient.SendPrivateMessage(new ChatMessage(this, packet.Message));
            }
            else
                SendServerMessage($"The player with the name \"{packet.DestinationPlayerName}\" doesn't exist.");
            */
        }


        /*
        private async Task HandleGameStateMessageAsync(GameStateMessagePacket packet)
        {
            var playerName = Module.GetClientName(packet.Origin);

            if (!string.IsNullOrEmpty(playerName))
                Module.ModuleManager.SendServerMessage($"The player {playerName} {packet.EventMessage}");
        }
        */

        /*
        private async Task HandleTradeRequestAsync(TradeRequestPacket packet)
        {
            var destClient = Module.GetClient(packet.DestinationPlayerID);
            if (destClient is P3DPlayer player)
            {
                if (GameMode == player.GameMode)
                {
                    // XNOR
                    if (IsGameJoltPlayer == player.IsGameJoltPlayer)
                        player.SendPacket(new TradeRequestPacket { Origin = packet.Origin });
                    else
                    {
                        SendServerMessage($"Can not start trade with {player.Name}! Online-Offline trade disabled.");
                        Module.OnTradeCancel(this, player);
                    }
                }
                else
                {
                    SendServerMessage($"Can not start trade with {player.Name}! Different GameModes used.");
                    Module.OnTradeCancel(this, player);
                }
            }
            else
                destClient.SendPacket(new TradeRequestPacket { Origin = packet.Origin });
        }
        private async Task HandleTradeJoinAsync(TradeJoinPacket packet)
        {
            Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new TradeJoinPacket { Origin = packet.Origin });
        }
        private async Task HandleTradeQuitAsync(TradeQuitPacket packet)
        {
            Module.OnTradeCancel(this, Module.GetClient(packet.DestinationPlayerID));
        }
        private async Task HandleTradeOfferAsync(TradeOfferPacket packet)
        {
            var destClient = Module.GetClient(packet.DestinationPlayerID);

            if (PokemonValid(packet.TradeData))
                Module.OnTradeRequest(this,
                //new Monster(new DataItems(packet.TradeData))
                packet.TradeData, destClient);
            else
            {
                SendServerMessage("Your Pokemon is not valid!");
                Module.OnTradeCancel(this, destClient);
            }
        }
        private async Task HandleTradeStartAsync(TradeStartPacket packet)
        {
            Module.OnTradeConfirm(this, Module.GetClient(packet.DestinationPlayerID));
        }
        */

        /*
        private async Task HandleBattleClientDataAsync(BattleClientDataPacket packet)
        {
            Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new BattleClientDataPacket { Origin = packet.Origin, DataItems = packet.BattleData });
        }
        private async Task HandleBattleHostDataAsync(BattleHostDataPacket packet)
        {
            Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new BattleHostDataPacket { Origin = packet.Origin, DataItems = packet.BattleData });
        }
        private async Task HandleBattleJoinAsync(BattleJoinPacket packet)
        {
            Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new BattleJoinPacket { Origin = packet.Origin });
        }
        private async Task HandleBattleOfferAsync(BattleOfferPacket packet)
        {
            if (PokemonsValid(packet.BattleData))
                Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new BattleOfferPacket { Origin = packet.Origin, DataItems = packet.BattleData });
            else
            {
                SendServerMessage("One of your Pokemon is not valid!");
                await _writer.WriteAsync(_protocol, new BattleQuitPacket { Origin = packet.DestinationPlayerID });
            }
        }
        private async Task HandleBattlePokemonDataAsync(BattleEndRoundDataPacket packet)
        {
            Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new BattleEndRoundDataPacket { Origin = packet.Origin, DataItems = packet.BattleData });
        }
        private async Task HandleBattleQuitAsync(BattleQuitPacket packet)
        {
            Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new BattleQuitPacket { Origin = packet.Origin });
        }
        private async Task HandleBattleRequestAsync(BattleRequestPacket packet)
        {
            Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new BattleRequestPacket { Origin = packet.Origin });
        }
        private async Task HandleBattleStartAsync(BattleStartPacket packet)
        {
            Module.GetClient(packet.DestinationPlayerID)?.SendPacket(new BattleStartPacket { Origin = packet.Origin });
        }
        */


        private async Task HandleServerDataRequestAsync(ServerDataRequestPacket packet, CancellationToken ct)
        {
            var clientNames = _playerHandlerService.Players.Select(x => x.Name).ToArray();
            await _writer.WriteAsync(_protocol, new ServerInfoDataPacket
            {
                Origin = Origin.Server,

                CurrentPlayers = clientNames.Length,
                MaxPlayers = _serverOptions.MaxPlayers,
                PlayerNames = clientNames,

                ServerName = _serverOptions.ServerName,
                ServerMessage = _serverOptions.ServerMessage,
            }, ct);

            _lifetimeNotificationFeature.RequestClose();
        }
    }
}