using P3D.Legacy.Common;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Models
{
    public record ServerPlayer : IPlayer
    {
        public string ConnectionId { get; } = "SERVER";
        public Origin Id { get; } = Origin.Server;
        public string Name { get; } = "Server";
        public ulong GameJoltId { get; } = 0;
        public Permissions Permissions { get; } = Permissions.Server;

        public Task AssignIdAsync(long id, CancellationToken ct) => throw new NotSupportedException();
        public Task AssignPermissionsAsync(Permissions permissions, CancellationToken ct) => throw new NotSupportedException();

        public void Deconstruct(out Origin id, out string name, out ulong gameJoltId, out string connectionId)
        {
            id = Id;
            name = Name;
            gameJoltId = GameJoltId;
            connectionId = ConnectionId;
        }
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

        Task AssignIdAsync(long id, CancellationToken ct);
        Task AssignPermissionsAsync(Permissions permissions, CancellationToken ct);
    }
}