using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Players
{
    public interface IPlayerQueries
    {
        Task<PlayerViewModel?> GetAsync(long id, CancellationToken ct);
        IAsyncEnumerable<PlayerViewModel> GetAllAsync(CancellationToken ct);
    }
}