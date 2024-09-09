using P3D.Legacy.Common;
using P3D.Legacy.Server.Domain.Entities.Statistics;
using P3D.Legacy.Server.Domain.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Statistics;

public sealed class DefaultStatisticsRepository : IStatisticsRepository
{
    private readonly LiteDbStatisticsRepository _liteDbStatisticsRepository;

    public DefaultStatisticsRepository(LiteDbStatisticsRepository liteDbStatisticsRepository)
    {
        _liteDbStatisticsRepository = liteDbStatisticsRepository ?? throw new ArgumentNullException(nameof(liteDbStatisticsRepository));
    }

    public Task<StatisticsEntity?> GetAsync(string action, CancellationToken ct)
    {
        return _liteDbStatisticsRepository.GetAsync(PlayerId.None, action, ct);
    }
    public Task<StatisticsEntity?> GetAsync(PlayerId id, string action, CancellationToken ct)
    {
        return _liteDbStatisticsRepository.GetAsync(id, action, ct);
    }

    IAsyncEnumerable<StatisticsEntity> IStatisticsRepository.GetAllAsync(CancellationToken ct)
    {
        return _liteDbStatisticsRepository.GetAllAsync(ct);
    }

    public IAsyncEnumerable<StatisticsEntity> GetAllAsync(PlayerId id, CancellationToken ct)
    {
        return _liteDbStatisticsRepository.GetAllAsync(id, ct);
    }

    public IAsyncEnumerable<StatisticsEntity> GetAllAsync(string action, CancellationToken ct)
    {
        return _liteDbStatisticsRepository.GetAllAsync(action, ct);
    }

    public Task<bool> IncrementActionAsync(string action, CancellationToken ct)
    {
        return _liteDbStatisticsRepository.IncrementActionAsync(PlayerId.None, action, ct);
    }

    public Task<bool> IncrementActionAsync(PlayerId id, string action, CancellationToken ct)
    {
        return _liteDbStatisticsRepository.IncrementActionAsync(id, action, ct);
    }
}