using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Bans;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories
{

    public interface IBanRepository
    {
        Task<BanEntity?> GetAsync(GameJoltId id, CancellationToken ct);
        IAsyncEnumerable<BanEntity> GetAllAsync(CancellationToken ct);

        Task<bool> UpsertAsync(BanEntity banEntity, CancellationToken ct);
        Task<bool> DeleteAsync(GameJoltId id, CancellationToken ct);
    }
}