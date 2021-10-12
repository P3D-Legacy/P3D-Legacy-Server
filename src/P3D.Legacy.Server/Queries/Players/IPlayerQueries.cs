using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Queries.Players
{
    public interface IPlayerQueries
    {
        Task<PlayerViewModel?> GetAsync(ulong id, CancellationToken ct);
        IAsyncEnumerable<PlayerViewModel> GetAllAsync(CancellationToken ct);
    }
}