using P3D.Legacy.Common;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Models
{
    public record ServerPlayer : IPlayer
    {
        public Origin Id { get; } = Origin.Server;
        public string Name { get; } = "Server";
        public ulong GameJoltId { get; } = 0;
        public string ConnectionId { get; } = "SERVER";

        public Task AssignIdAsync(long id, CancellationToken ct) => throw new NotSupportedException();

        public void Deconstruct(out Origin id, out string name, out ulong gameJoltId, out string connectionId)
        {
            id = Id;
            name = Name;
            gameJoltId = GameJoltId;
            connectionId = ConnectionId;
        }
    }

    public interface IPlayer
    {
        static IPlayer Server { get; } = new ServerPlayer();

        string ConnectionId { get; }

        Origin Id { get; }
        string Name { get; }
        ulong GameJoltId { get; }

        Task AssignIdAsync(long id, CancellationToken ct);
    }
}