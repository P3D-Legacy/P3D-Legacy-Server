using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;

using System.Net;
using System.Numerics;

namespace P3D.Legacy.Server.Client.P3D
{
    // ReSharper disable once ArrangeTypeModifiers
    partial class P3DConnectionContextHandler : IP3DPlayerState
    {
        public PlayerId Id { get; private set; } = PlayerId.None;
        public Origin Origin { get; private set; } = Origin.None;
        public string Name { get; private set; } = string.Empty;
        public GameJoltId GameJoltId { get; private set; } = GameJoltId.None;
        public PermissionTypes Permissions { get; private set; } = PermissionTypes.UnVerified;
        public IPEndPoint IPEndPoint { get; private set; } = new(IPAddress.None, 0);
        public PlayerState State { get; private set; } = PlayerState.None;


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