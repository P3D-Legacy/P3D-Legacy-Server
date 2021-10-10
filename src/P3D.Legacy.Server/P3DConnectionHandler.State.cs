using System.Globalization;
using System.Numerics;

namespace P3D.Legacy.Server
{
    public partial class P3DConnectionHandler
    {
        private static CultureInfo CultureInfo => CultureInfo.InvariantCulture;

        public uint Id { get; set; }
        public string Name { get; protected set; }

        public string GameMode { get; private set; }
        public bool IsGameJoltPlayer { get; private set; }
        public long GameJoltID { get; private set; }
        private char DecimalSeparator { get; set; }


        public string LevelFile { get; set; }
        public Vector3 Position { get; set; }
        public int Facing { get; private set; }
        public bool Moving { get; private set; }

        public string Skin { get; private set; }
        public string BusyType { get; private set; }

        public bool PokemonVisible { get; private set; }
        public Vector3 PokemonPosition { get; private set; }
        public string PokemonSkin { get; private set; }
        public int PokemonFacing { get; private set; }
    }
}