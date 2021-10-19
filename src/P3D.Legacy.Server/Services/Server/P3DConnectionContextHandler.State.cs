using Microsoft.AspNetCore.Http.Features;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;

using System.Net;
using System.Numerics;

namespace P3D.Legacy.Server.Services.Server
{
    public sealed partial class P3DConnectionContextHandler : IP3DPlayerState
    {
        public string ConnectionId { get; private set; } = default!;

        public Origin Id { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public GameJoltId GameJoltId { get; private set; } = default!;
        public PermissionFlags Permissions { get; private set; } = default!;
        public IPAddress IPAddress { get; private set; } = default!;

        public IFeatureCollection Features { get; private set; } = new FeatureCollection();


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