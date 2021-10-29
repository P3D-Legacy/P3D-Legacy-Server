using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;

using System.Net;
using System.Numerics;

namespace P3D.Legacy.Server.Client.P3D
{
    partial class P3DConnectionContextHandler : IP3DPlayerState
    {
        public Origin Id { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public GameJoltId GameJoltId { get; private set; } = default!;
        public PermissionFlags Permissions { get; private set; } = default!;
        public IPEndPoint IPEndPoint { get; private set; } = default!;


        public string GameMode { get; set; } = default!;
        public bool IsGameJoltPlayer { get; set; } = default!;
        public char DecimalSeparator { get; set; } = default!;

        public string LevelFile { get; set; } = default!;
        public Vector3 Position { get; set; } = default!;
        public int Facing { get; set; } = default!;
        public bool Moving { get; set; } = default!;

        public string Skin { get; set; } = default!;
        public string BusyType { get; set; } = default!;

        public bool MonsterVisible { get; set; } = default!;
        public Vector3 MonsterPosition { get; set; } = default!;
        public string MonsterSkin { get; set; } = default!;
        public int MonsterFacing { get; set; } = default!;

    }
}