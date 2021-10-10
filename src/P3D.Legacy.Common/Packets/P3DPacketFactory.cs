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
        public P3DPacket? GetFromId(P3DPacketTypes id) => id switch
        {
            P3DPacketTypes.GameData => new GameDataPacket(),
            P3DPacketTypes.NOT_USED => null,
            P3DPacketTypes.ChatMessagePrivate => new ChatMessagePrivatePacket(),
            P3DPacketTypes.ChatMessageGlobal => new ChatMessageGlobalPacket(),
            P3DPacketTypes.Kicked => new KickedPacket(),
            P3DPacketTypes.ID => new IDPacket(),
            P3DPacketTypes.CreatePlayer => new CreatePlayerPacket(),
            P3DPacketTypes.DestroyPlayer => new DestroyPlayerPacket(),
            P3DPacketTypes.ServerClose => new ServerClosePacket(),
            P3DPacketTypes.ServerMessage => new ServerMessagePacket(),
            P3DPacketTypes.WorldData => new WorldDataPacket(),
            P3DPacketTypes.Ping => new PingPacket(),
            P3DPacketTypes.GameStateMessage => new GameStateMessagePacket(),
            P3DPacketTypes.TradeRequest => new TradeRequestPacket(),
            P3DPacketTypes.TradeJoin => new TradeJoinPacket(),
            P3DPacketTypes.TradeQuit => new TradeQuitPacket(),
            P3DPacketTypes.TradeOffer => new TradeOfferPacket(),
            P3DPacketTypes.TradeStart => new TradeStartPacket(),
            P3DPacketTypes.BattleRequest => new BattleRequestPacket(),
            P3DPacketTypes.BattleJoin => new BattleJoinPacket(),
            P3DPacketTypes.BattleQuit => new BattleQuitPacket(),
            P3DPacketTypes.BattleOffer => new BattleOfferPacket(),
            P3DPacketTypes.BattleStart => new BattleStartPacket(),
            P3DPacketTypes.BattleClientData => new BattleClientDataPacket(),
            P3DPacketTypes.BattleHostData => new BattleHostDataPacket(),
            P3DPacketTypes.BattleEndRoundData => new BattleEndRoundDataPacket(),
            P3DPacketTypes.ServerInfoData => new ServerInfoDataPacket(),
            P3DPacketTypes.ServerDataRequest => new ServerDataRequestPacket(),
            _ => null
        };
    }
}
