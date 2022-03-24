using System.Linq;

namespace P3D.Legacy.Common.Packets.Server
{
    public sealed partial record ServerInfoDataPacket() : P3DPacket(P3DPacketType.ServerInfoData)
    {
        [P3DPacketDataItem(0, DataItemType.Int32)]
        public int CurrentPlayers { get; set; }
        [P3DPacketDataItem(1, DataItemType.Int32)]
        public int MaxPlayers { get; set; }
        [P3DPacketDataItem(2, DataItemType.String)]
        public string ServerName { get; set; }
        [P3DPacketDataItem(3, DataItemType.String)]
        public string ServerMessage { get; set; }
        [P3DPacketDataItem(4, DataItemType.StringArray)]
        public string[] PlayerNames
        {
            get => DataItemStorage.Skip(4).ToArray();
            init
            {
                for (var i = 0; i < value.Length; i++)
                    DataItemStorage.Set(4 + i, value[i]);
            }
        }

        public void Deconstruct(out int currentPlayers, out int maxPlayers, out string serverName, out string serverMessage, out string[] playerNames)
        {
            currentPlayers = CurrentPlayers;
            maxPlayers = MaxPlayers;
            serverName = ServerName;
            serverMessage = ServerMessage;
            playerNames = PlayerNames;
        }
    }
}