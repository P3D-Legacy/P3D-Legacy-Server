using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Statistics;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Services.Statistics
{
    public interface IStatisticsManager
    {
        Task<StatisticsEntity?> GetAsync(string action, CancellationToken ct);
        Task<StatisticsEntity?> GetAsync(PlayerId id, string action, CancellationToken ct);
        IAsyncEnumerable<StatisticsEntity> GetAllAsync(CancellationToken ct);
        IAsyncEnumerable<StatisticsEntity> GetAllAsync(PlayerId id, CancellationToken ct);
        IAsyncEnumerable<StatisticsEntity> GetAllAsync(string action, CancellationToken ct);

        Task<bool> IncrementActionAsync(string action, CancellationToken ct);
        Task<bool> IncrementActionAsync(PlayerId id, string action, CancellationToken ct);
    }
}
