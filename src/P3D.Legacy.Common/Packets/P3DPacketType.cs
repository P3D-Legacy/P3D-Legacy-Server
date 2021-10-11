namespace P3D.Legacy.Common.Packets
{
    public enum P3DPacketType : byte
    {
#pragma warning disable format
        None                        = 0xFF,
        GameData                    = 0x00,
        NotUsed                     = 0x01,
        ChatMessagePrivate          = 0x02,
        ChatMessageGlobal           = 0x03,
        Kicked                      = 0x04,

        Id                          = 0x07,
        CreatePlayer                = 0x08,
        DestroyPlayer               = 0x09,
        ServerClose                 = 0x0A,
        ServerMessage               = 0x0B,
        WorldData                   = 0x0C,
        Ping                        = 0x0D,
        GameStateMessage            = 0x0E,

        TradeRequest                = 0x1E,
        TradeJoin                   = 0x1F,
        TradeQuit                   = 0x20,

        TradeOffer                  = 0x21,
        TradeStart                  = 0x22,

        BattleRequest               = 0x32,
        BattleJoin                  = 0x33,
        BattleQuit                  = 0x34,

        BattleOffer                 = 0x35,
        BattleStart                 = 0x36,

        BattleClientData            = 0x37,
        BattleHostData              = 0x38,
        BattleEndRoundData          = 0x39,

        ServerInfoData              = 0x62,
        ServerDataRequest           = 0x63
#pragma warning restore format
    }
}