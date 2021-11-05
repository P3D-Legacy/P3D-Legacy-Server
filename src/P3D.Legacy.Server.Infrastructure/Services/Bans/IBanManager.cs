using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Bans;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Services.Bans
{
    public interface IBanManager
    {
        Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct);
        IAsyncEnumerable<BanEntity> GetAllAsync(CancellationToken ct);

        Task<bool> BanAsync(BanEntity banEntity, CancellationToken ct);
        Task<bool> UnbanAsync(PlayerId id, CancellationToken ct);
    }
}
