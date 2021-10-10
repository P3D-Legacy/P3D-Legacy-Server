using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using P3D.Legacy.Common.Extensions;

namespace P3D.Legacy.Common.Packets.Shared
{
    public class GameDataPacket : P3DPacket
    {
        public override P3DPacketTypes Id => P3DPacketTypes.GameData;

        public string GameMode { get => DataItems[0]; set => DataItems[0] = value; }
        public bool IsGameJoltPlayer { get => int.TryParse(DataItems[1], NumberStyles.Any, CultureInfo, out var isGameJoltPlayer) && isGameJoltPlayer == 1; set => DataItems[1] = (value ? 1 : 2).ToString(CultureInfo); }
        public long GameJoltID { get => long.TryParse(DataItems[2], NumberStyles.Any, CultureInfo, out var gameJoltID) ? gameJoltID : 0; set => DataItems[2] = value.ToString(CultureInfo); }
        public char DecimalSeparator { get => DataItems[3].Any() ? DataItems[3][0] : ','; set => DataItems[3] = value.ToString(); }
        public string Name { get => DataItems[4]; set => DataItems[4] = value; }
        public string LevelFile { get => DataItems[5]; set => DataItems[5] = value; }
        private string Position { get => DataItems[6]; set => DataItems[6] = value; }
        public int Facing { get => int.TryParse(DataItems[7], NumberStyles.Any, CultureInfo, out var facing) ? facing : 0; set => DataItems[7] = value.ToString(CultureInfo); }
        public bool Moving { get => int.TryParse(DataItems[8], NumberStyles.Any, CultureInfo, out var moving) && moving == 1; set => DataItems[8] = (value ? 1 : 2).ToString(CultureInfo); }
        public string Skin { get => DataItems[9]; set => DataItems[9] = value; }
        public string BusyType { get => DataItems[10]; set => DataItems[10] = value; }
        public bool PokemonVisible { get => int.TryParse(DataItems[11], NumberStyles.Any, CultureInfo, out var visible) && visible == 1; set => DataItems[11] = (value ? 1 : 2).ToString(CultureInfo); }
        private string PokemonPosition { get => DataItems[12]; set => DataItems[12] = value; }
        public string PokemonSkin { get => DataItems[13]; set => DataItems[13] = value; }
        public int PokemonFacing { get { try { return int.TryParse(DataItems[14], NumberStyles.Any, CultureInfo, out var facing) ? facing : 0; } catch (Exception) { return 0; } } set => DataItems[14] = value.ToString(CultureInfo); }


        public Vector3 GetPosition(char separator) { return Vector3Extensions.FromP3DString(Position, separator, CultureInfo); }
        public void SetPosition(Vector3 position, char separator) { Position = position.ToP3DString(separator, CultureInfo); }

        public Vector3 GetPokemonPosition(char separator) { return Vector3Extensions.FromP3DString(PokemonPosition, separator, CultureInfo); }
        public void SetPokemonPosition(Vector3 position, char separator) { PokemonPosition = position.ToP3DString(separator, CultureInfo); }
    }
}
