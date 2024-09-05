using P3D.Legacy.Server.Client.P3D.Packets.Battle;
using P3D.Legacy.Server.Client.P3D.Packets.Chat;
using P3D.Legacy.Server.Client.P3D.Packets.Client;
using P3D.Legacy.Server.Client.P3D.Packets.Common;
using P3D.Legacy.Server.Client.P3D.Packets.Server;
using P3D.Legacy.Server.Client.P3D.Packets.Trade;

namespace P3D.Legacy.Server.Client.P3D.Packets;

public interface IP3DPacketFactory
{
    public P3DPacket? GetFromId(P3DPacketType id);
}

public class P3DPacketClientFactory : IP3DPacketFactory
{
    public P3DPacket? GetFromId(P3DPacketType id) => id switch
    {
        P3DPacketType.GameData => new GameDataPacket(),
        P3DPacketType.NotUsed => null,
        P3DPacketType.ChatMessagePrivate => new ChatMessagePrivateToClientPacket(),
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
        P3DPacketType.TradeOffer => new TradeOfferToClientPacket(),
        P3DPacketType.TradeStart => new TradeStartPacket(),
        P3DPacketType.BattleRequest => new BattleRequestPacket(),
        P3DPacketType.BattleJoin => new BattleJoinPacket(),
        P3DPacketType.BattleQuit => new BattleQuitPacket(),
        P3DPacketType.BattleOffer => new BattleOfferToClientPacket(),
        P3DPacketType.BattleStart => new BattleStartPacket(),
        P3DPacketType.BattleClientData => new BattleClientDataToClientPacket(),
        P3DPacketType.BattleHostData => new BattleHostDataToClientPacket(),
        P3DPacketType.BattleEndRoundData => new BattleHostEndRoundDataToClientPacket(),
        P3DPacketType.ServerInfoData => new ServerInfoDataPacket(),
        P3DPacketType.ServerDataRequest => new ServerDataRequestPacket(),
        _ => null
    };
}

public class P3DPacketServerFactory : IP3DPacketFactory
{
    public P3DPacket? GetFromId(P3DPacketType id) => id switch
    {
        P3DPacketType.GameData => new GameDataPacket(),
        P3DPacketType.NotUsed => null,
        P3DPacketType.ChatMessagePrivate => new ChatMessagePrivateFromClientPacket(),
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
        P3DPacketType.BattleClientData => new BattleClientDataFromClientPacket(),
        P3DPacketType.BattleHostData => new BattleHostDataFromClientPacket(),
        P3DPacketType.BattleEndRoundData => new BattleHostEndRoundDataFromClientPacket(),
        P3DPacketType.ServerInfoData => new ServerInfoDataPacket(),
        P3DPacketType.ServerDataRequest => new ServerDataRequestPacket(),
        _ => null
    };
}