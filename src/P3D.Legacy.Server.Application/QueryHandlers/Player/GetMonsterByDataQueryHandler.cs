using Microsoft.Extensions.Logging;

using P3D.Legacy.Common.Monsters;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.CQERS.Queries;
using P3D.Legacy.Server.Infrastructure.Repositories.Monsters;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class GetMonsterByDataQueryHandler : IQueryHandler<GetMonsterByDataQuery, IMonsterInstance>
    {
        private readonly ILogger _logger;
        private readonly IMonsterRepository _monsterRepository;

        public GetMonsterByDataQueryHandler(ILogger<GetMonsterByDataQueryHandler> logger, IMonsterRepository monsterRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _monsterRepository = monsterRepository ?? throw new ArgumentNullException(nameof(monsterRepository));
        }

        public async Task<IMonsterInstance> HandleAsync(GetMonsterByDataQuery query, CancellationToken ct)
        {
            var monsterData = query.MonsterData;

            return await _monsterRepository.GetByDataAsync(monsterData, ct);
        }
    }
}