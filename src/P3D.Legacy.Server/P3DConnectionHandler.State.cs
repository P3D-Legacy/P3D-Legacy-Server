using System;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public partial class P3DConnectionHandler
    {
        public ulong Id { get; set; }
        public string Name { get; protected set; }


        private string GameMode { get; set; }
        private bool IsGameJoltPlayer { get; set; }
        private ulong GameJoltId { get; set; }
        private char DecimalSeparator { get; set; }

        private string LevelFile { get; set; }
        private Vector3 Position { get; set; }
        private int Facing { get; set; }
        private bool Moving { get; set; }

        private string Skin { get; set; }
        private string BusyType { get; set; }

        private bool PokemonVisible { get; set; }
        private Vector3 PokemonPosition { get; set; }
        private string PokemonSkin { get; set; }
        private int PokemonFacing { get; set; }

        private void ResetState()
        {
            Id = 0;
            Name = string.Empty;

            GameMode = string.Empty;
            IsGameJoltPlayer = false;
            GameJoltId = 0;
            DecimalSeparator = char.MinValue;

            LevelFile = string.Empty;
            Position = Vector3.Zero;
            Facing = 0;
            Moving = false;

            Skin = string.Empty;
            BusyType = string.Empty;

            PokemonVisible = false;
            PokemonPosition = Vector3.Zero;
            PokemonSkin = string.Empty;
            PokemonFacing = 0;
        }
    }
}