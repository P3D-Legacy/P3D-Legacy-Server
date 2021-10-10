using System;
using System.Collections.Generic;

namespace P3D.Legacy.Common.Packets.Server
{
    public class ServerInfoDataPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.ServerInfoData;

        public int CurrentPlayers { get => int.Parse(DataItems[0] == string.Empty ? 0.ToString() : DataItems[0]); init => DataItems[0] = value.ToString(); }
        public int MaxPlayers { get => int.Parse(DataItems[1] == string.Empty ? 0.ToString() : DataItems[1]); init => DataItems[1] = value.ToString(); }
        public string ServerName { get => DataItems[2]; init => DataItems[2] = value; }
        public string ServerMessage { get => DataItems[3]; init => DataItems[3] = value; }
        public string[] PlayerNames
        {
            get
            {
                if (DataItems.Length > 4)
                {
                    var list = new List<string>();
                    for (var i = 4; i < DataItems.Length; i++)
                        list.Add(DataItems[i]);

                    return list.ToArray();
                }

                return Array.Empty<string>();
            }
            init
            {
                if (value != null)
                {
                    for (var i = 0; i < value.Length; i++)
                        DataItems[4 + i] = value[i];
                }
            }
        }
    }
}
