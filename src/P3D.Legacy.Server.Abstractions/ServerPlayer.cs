using P3D.Legacy.Common;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions
{
    public sealed record ServerPlayer : IPlayer
    {
        public string ConnectionId => "SERVER";
        public PlayerId Id => PlayerId.None;
        public Origin Origin => Origin.Server;
        public string Name => "Server";
        public GameJoltId GameJoltId => GameJoltId.None;
        public PermissionFlags Permissions => PermissionFlags.Server;
        public IPEndPoint IPEndPoint => new(IPAddress.None, 0);

        public Task AssignIdAsync(PlayerId id, CancellationToken ct) => throw new NotSupportedException();
        public Task AssignOriginAsync(Origin origin, CancellationToken ct) => throw new NotSupportedException();
        public Task AssignPermissionsAsync(PermissionFlags permissions, CancellationToken ct) => throw new NotSupportedException();
        public Task KickAsync(string reason, CancellationToken ct) => throw new NotSupportedException();
    }
}