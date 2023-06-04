using P3D.Legacy.Common;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions
{
    public interface IPlayer
    {
        private sealed record ServerPlayer : IPlayer
        {
            public string ConnectionId => "SERVER";
            public PlayerId Id => PlayerId.None;
            public Origin Origin => Origin.Server;
            public string Name => "Server";
            public PermissionTypes Permissions => PermissionTypes.Server;
            public IPEndPoint IPEndPoint => new(IPAddress.None, 0);
            public PlayerState State => PlayerState.Initialized;

            public Task<GameJoltId> GetGameJoltIdOrNoneAsync(CancellationToken ct) => Task.FromResult(GameJoltId.None);
            public Task AssignIdAsync(PlayerId id, CancellationToken ct) => throw new NotSupportedException();
            public Task AssignOriginAsync(Origin origin, CancellationToken ct) => throw new NotSupportedException();
            public Task AssignPermissionsAsync(PermissionTypes permissions, CancellationToken ct) => throw new NotSupportedException();
            public Task KickAsync(string reason, CancellationToken ct) => throw new NotSupportedException();
        }

        static IPlayer Server { get; } = new ServerPlayer();

        string ConnectionId { get; }

        PlayerId Id { get; }
        Origin Origin { get; }
        string Name { get; }
        PermissionTypes Permissions { get; }
        IPEndPoint IPEndPoint { get; }
        PlayerState State { get; }

        Task<GameJoltId> GetGameJoltIdOrNoneAsync(CancellationToken ct);
        Task AssignIdAsync(PlayerId id, CancellationToken ct);
        Task AssignOriginAsync(Origin origin, CancellationToken ct);
        Task AssignPermissionsAsync(PermissionTypes permissions, CancellationToken ct);

        Task KickAsync(string reason, CancellationToken ct);
    }
}