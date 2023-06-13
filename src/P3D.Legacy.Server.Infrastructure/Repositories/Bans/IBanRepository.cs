using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Bans;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Bans
{
    public interface IBanRepository
    {
        Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct);

        Task<bool> BanAsync(BanEntity banEntity, CancellationToken ct);
        Task<bool> UnbanAsync(PlayerId id, CancellationToken ct);
    }
}
