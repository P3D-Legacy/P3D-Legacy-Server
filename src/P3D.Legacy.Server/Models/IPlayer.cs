using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Models
{
    public interface IPlayer
    {
        string ConnectionId { get; }

        ulong Id { get; }
        string Name { get; }
        ulong GameJoltId { get; }

        Task AssignIdAsync(ulong id, CancellationToken ct);
        //Task NotifyAsync(Event @event, CancellationToken ct);
    }
}