using P3D.Legacy.Common.Packets.Battle;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Shared;
using P3D.Legacy.Common.Packets.Trade;

namespace P3D.Legacy.Common.Packets
{
    public class P3DPacketFactory
    {
        public P3DPacket? GetFromId(P3DPacketType id) => id switch
        {
            P3DPacketType.GameData => new GameDataPacket(),
            P3DPacketType.NotUsed => null,
            P3DPacketType.ChatMessagePrivate => new ChatMessagePrivatePacket(),
            P3DPacketType.ChatMessageGlobal => new ChatMessageGlobalPacket(),
            P3DPacketType.Kicked => new KickedPacket(),
            P3DPacketType.Id => new IdPacket(),
            P3DPacketType.CreatePlayer => new CreatePlayerPacket(),
            P3DPacketType.DestroyPlayer => new DestroyPlayerPacket(),
            P3DPacketType.ServerClose => new ServerClosePacket(),
            P3DPacketType.ServerMessage => new ServerMessagePacket(),
            P3DPacketType.WorldData => new WorldDataPacket(),
            P3DPacketType.Ping => new PingPacket(),
            P3DPacketType.GameStateMessage => new GameStateMessagePacket(),
            P3DPacketType.TradeRequest => new TradeRequestPacket(),
            P3DPacketType.TradeJoin => new TradeJoinPacket(),
            P3DPacketType.TradeQuit => new TradeQuitPacket(),
            P3DPacketType.TradeOffer => new TradeOfferFromClientPacket(),
            P3DPacketType.TradeStart => new TradeStartPacket(),
            P3DPacketType.BattleRequest => new BattleRequestPacket(),
            P3DPacketType.BattleJoin => new BattleJoinPacket(),
            P3DPacketType.BattleQuit => new BattleQuitPacket(),
            P3DPacketType.BattleOffer => new BattleOfferFromClientPacket(),
            P3DPacketType.BattleStart => new BattleStartPacket(),
            P3DPacketType.BattleClientData => new BattleClientDataPacket(),
            P3DPacketType.BattleHostData => new BattleHostDataPacket(),
            P3DPacketType.BattleEndRoundData => new BattleEndRoundDataPacket(),
            P3DPacketType.ServerInfoData => new ServerInfoDataPacket(),
            P3DPacketType.ServerDataRequest => new ServerDataRequestPacket(),
            _ => null
        };
    }
}