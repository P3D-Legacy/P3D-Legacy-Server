using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Shared;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public partial class P3DConnectionHandler
    {
        private async Task HandlePacketAsync(P3DPacket? packet, CancellationToken ct)
        {
            switch (packet)
            {
                /*
                case BattleClientDataPacket battleClientDataPacket:
                    await HandleBattleClientDataAsync(battleClientDataPacket);
                    break;
                case BattleHostDataPacket battleHostDataPacket:
                    await HandleBattleHostDataAsync(battleHostDataPacket);
                    break;
                case BattleJoinPacket battleJoinPacket:
                    await HandleBattleJoinAsync(battleJoinPacket);
                    break;
                case BattleOfferPacket battleOfferPacket:
                    await HandleBattleOfferAsync(battleOfferPacket);
                    break;
                case BattleEndRoundDataPacket battleEndRoundDataPacket:
                    await HandleBattlePokemonDataAsync(battleEndRoundDataPacket);
                    break;
                case BattleQuitPacket battleQuitPacket:
                    await HandleBattleQuitAsync(battleQuitPacket);
                    break;
                case BattleRequestPacket battleRequestPacket:
                    await HandleBattleRequestAsync(battleRequestPacket);
                    break;
                case BattleStartPacket battleStartPacket:
                    await HandleBattleStartAsync(battleStartPacket);
                    break;
                */

                case ChatMessageGlobalPacket chatMessageGlobalPacket:
                    await HandleChatMessageAsync(chatMessageGlobalPacket);
                    break;
                case ChatMessagePrivatePacket chatMessagePrivatePacket:
                    await HandlePrivateMessageAsync(chatMessagePrivatePacket);
                    break;

                case ServerDataRequestPacket serverDataRequestPacket:
                    await HandleServerDataRequestAsync(serverDataRequestPacket, ct);
                    break;
                case GameDataPacket gameDataPacket:
                    await HandleGameDataAsync(gameDataPacket, ct);
                    break;
                    /*
                    case GameStateMessagePacket gameStateMessagePacket:
                        await HandleGameStateMessageAsync(gameStateMessagePacket);
                        break;
                    */
                    /*
                    case TradeJoinPacket tradeJoinPacket:
                        await HandleTradeJoinAsync(tradeJoinPacket);
                        break;
                    case TradeOfferPacket tradeOfferPacket:
                        await HandleTradeOfferAsync(tradeOfferPacket);
                        break;
                    case TradeQuitPacket tradeQuitPacket:
                        await HandleTradeQuitAsync(tradeQuitPacket);
                        break;
                    case TradeRequestPacket tradeRequestPacket:
                        await HandleTradeRequestAsync(tradeRequestPacket);
                        break;
                    case TradeStartPacket tradeStartPacket:
                        await HandleTradeStartAsync(tradeStartPacket);
                        break;
                    */
            }
        }

        public async Task SendServerMessageAsync(string text) => await _writer.WriteAsync(_protocol, new ChatMessageGlobalPacket
        {
            Origin = Origin.Server, Message = text
        });
    }
}