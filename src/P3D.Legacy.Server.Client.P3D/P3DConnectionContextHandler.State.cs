using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;

using System.Net;
using System.Numerics;

namespace P3D.Legacy.Server.Client.P3D
{
    // ReSharper disable once ArrangeTypeModifiers
    partial class P3DConnectionContextHandler : IP3DPlayerState
    {
        public Origin Id { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public GameJoltId GameJoltId { get; private set; } = default!;
        public PermissionFlags Permissions { get; private set; } = default!;
        public IPEndPoint IPEndPoint { get; private set; } = default!;


        public string GameMode { get; private set; } = default!;
        public bool IsGameJoltPlayer { get; private set; } = default!;
        public char DecimalSeparator { get; private set; } = default!;

        public string LevelFile { get; private set; } = default!;
        public Vector3 Position { get; private set; } = default!;
        public int Facing { get; private set; } = default!;
        public bool Moving { get; private set; } = default!;

        public string Skin { get; private set; } = default!;
        public string BusyType { get; private set; } = default!;

        public bool MonsterVisible { get; private set; } = default!;
        public Vector3 MonsterPosition { get; private set; } = default!;
        public string MonsterSkin { get; private set; } = default!;
        public int MonsterFacing { get; private set; } = default!;

    }
}