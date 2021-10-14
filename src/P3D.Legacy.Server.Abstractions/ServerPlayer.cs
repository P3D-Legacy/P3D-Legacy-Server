using Microsoft.AspNetCore.Http.Features;

using P3D.Legacy.Common;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions
{
    public sealed record ServerPlayer : IPlayer
    {
        public string ConnectionId => "SERVER";
        public Origin Id => Origin.Server;
        public string Name => "Server";
        public GameJoltId GameJoltId => GameJoltId.None;
        public Permissions Permissions => Permissions.Server;

        public IFeatureCollection Features { get; } = new FeatureCollection();

        public Task AssignIdAsync(long id, CancellationToken ct) => throw new NotSupportedException();
        public Task AssignPermissionsAsync(Permissions permissions, CancellationToken ct) => throw new NotSupportedException();
    }
}