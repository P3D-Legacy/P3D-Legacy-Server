using P3D.Legacy.Common;
using P3D.Legacy.Server.Models;

using System.Numerics;

namespace P3D.Legacy.Server.Services.Server
{
    public partial class P3DConnectionContextHandler
    {
        public string ConnectionId { get; private set; } = default!;

        public Origin Id { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public ulong GameJoltId { get; private set; } = default!;
        public Permissions Permissions { get; private set; } = default!;


        private string GameMode { get; set; } = default!;
        private bool IsGameJoltPlayer { get; set; } = default!;
        private char DecimalSeparator { get; set; } = default!;

        private string LevelFile { get; set; } = default!;
        private Vector3 Position { get; set; } = default!;
        private int Facing { get; set; } = default!;
        private bool Moving { get; set; } = default!;

        private string Skin { get; set; } = default!;
        private string BusyType { get; set; } = default!;

        private bool MonsterVisible { get; set; } = default!;
        private Vector3 MonsterPosition { get; set; } = default!;
        private string MonsterSkin { get; set; } = default!;
        private int MonsterFacing { get; set; } = default!;
    }
}
