/*
using Microsoft.Extensions.Options;

using P3D.Legacy.Common.Monsters;
using P3D.Legacy.Server.Abstractions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Monsters
{
    public sealed class MonsterRepository : IMonsterRepository
    {
        private readonly IMonsterRepository _implementation;

        public MonsterRepository(IOptions<ServerOptions> serverOptions, PokeAPIMonsterRepository pokeAPIMonsterRepository, NopMonsterRepository nopMonsterRepository)
        {
            if (serverOptions.Value is not { } options)
                throw new ArgumentNullException(nameof(serverOptions));

            _implementation = options.ValidationEnabled
                ? pokeAPIMonsterRepository
                : nopMonsterRepository;
        }

        public Task<IMonsterInstance> GetByDataAsync(string monsterData, CancellationToken ct) => _implementation.GetByDataAsync(monsterData, ct);
    }
}
*/