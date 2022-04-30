using System.Collections.Immutable;
using System.Linq;

namespace P3D.Legacy.Common.Packets.Server
{
    public sealed record ServerInfoDataPacket() : P3DPacket(P3DPacketType.ServerInfoData)
    {
        public int CurrentPlayers { get => DataItemStorage.GetInt32(0); init => DataItemStorage.Set(0, value); }
        public int MaxPlayers { get => DataItemStorage.GetInt32(1); init => DataItemStorage.Set(1, value); }
        public string ServerName { get => DataItemStorage.Get(2); init => DataItemStorage.Set(2, value); }
        public string ServerMessage { get => DataItemStorage.Get(3); init => DataItemStorage.Set(3, value); }
        public ImmutableArray<string> PlayerNames
        {
            get => DataItemStorage.Skip(4).ToImmutableArray();
            init
            {
                for (var i = 0; i < value.Length; i++)
                    DataItemStorage.Set(4 + i, value[i]);
            }
        }

        public void Deconstruct(out int currentPlayers, out int maxPlayers, out string serverName, out string serverMessage, out ImmutableArray<string> playerNames)
        {
            currentPlayers = CurrentPlayers;
            maxPlayers = MaxPlayers;
            serverName = ServerName;
            serverMessage = ServerMessage;
            playerNames = PlayerNames;
        }
    }
}