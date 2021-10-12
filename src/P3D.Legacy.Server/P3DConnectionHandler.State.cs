using System;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public partial class P3DConnectionHandler
    {
        public ulong Id { get; private set; }
        public string Name { get; private set; }
        public ulong GameJoltId { get; private set; }


        private string GameMode { get; set; }
        private bool IsGameJoltPlayer { get; set; }
        private char DecimalSeparator { get; set; }

        private string LevelFile { get; set; }
        private Vector3 Position { get; set; }
        private int Facing { get; set; }
        private bool Moving { get; set; }

        private string Skin { get; set; }
        private string BusyType { get; set; }

        private bool MonsterVisible { get; set; }
        private Vector3 MonsterPosition { get; set; }
        private string MonsterSkin { get; set; }
        private int MonsterFacing { get; set; }

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

            MonsterVisible = false;
            MonsterPosition = Vector3.Zero;
            MonsterSkin = string.Empty;
            MonsterFacing = 0;
        }
    }
}