using P3D.Legacy.Common;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Bans
{
    public interface IBanQueries
    {
        Task<BanViewModel?> GetAsync(PlayerId id, CancellationToken ct);
        IAsyncEnumerable<BanViewModel> GetAllAsync(CancellationToken ct);
    }
}