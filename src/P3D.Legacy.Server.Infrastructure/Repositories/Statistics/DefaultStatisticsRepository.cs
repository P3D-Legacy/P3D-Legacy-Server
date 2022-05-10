using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Infrastructure.Models.Statistics;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Statistics
{
    public sealed class DefaultStatisticsRepository : IStatisticsRepository
    {
        private readonly ServerOptions _options;
        private readonly LiteDbStatisticsRepository _liteDbStatisticsRepository;

        public DefaultStatisticsRepository(IOptions<ServerOptions> options, LiteDbStatisticsRepository liteDbStatisticsRepository)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
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
}