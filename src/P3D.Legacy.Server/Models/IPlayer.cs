using Microsoft.AspNetCore.Http.Features;

using P3D.Legacy.Common;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Models
{
    public record ServerPlayer : IPlayer
    {
        public string ConnectionId => "SERVER";
        public Origin Id => Origin.Server;
        public string Name => "Server";
        public ulong GameJoltId => 0;
        public Permissions Permissions => Permissions.Server;

        public IFeatureCollection Features { get; } = new FeatureCollection();

        public Task AssignIdAsync(long id, CancellationToken ct) => throw new NotSupportedException();
        public Task AssignPermissionsAsync(Permissions permissions, CancellationToken ct) => throw new NotSupportedException();
    }

    [Flags]
    public enum Permissions
    {
        None                    = 0,
        UnVerified              = 1,
        User                    = 2,
        Moderator               = 4,
        Administrator           = 8,
        Server                  = 16,


        UnVerifiedOrHigher      = UnVerified | User | Moderator | Administrator | Server,
        UserOrHigher            = User | Moderator | Administrator | Server,
        ModeratorOrHigher       = Moderator | Administrator | Server,
        AdministratorOrHigher   = Administrator | Server,
    }

    public interface IPlayer
    {
        static IPlayer Server { get; } = new ServerPlayer();

        string ConnectionId { get; }

        Origin Id { get; }
        string Name { get; }
        ulong GameJoltId { get; }
        Permissions Permissions { get; }

        IFeatureCollection Features { get; }

        Task AssignIdAsync(long id, CancellationToken ct);
        Task AssignPermissionsAsync(Permissions permissions, CancellationToken ct);
    }
}