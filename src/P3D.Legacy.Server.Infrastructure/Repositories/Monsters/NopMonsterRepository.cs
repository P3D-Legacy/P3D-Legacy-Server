using P3D.Legacy.Common.Monsters;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Monsters
{
    public class NopMonsterRepository : IMonsterRepository
    {
        private sealed record NopMonsterInstance : IMonsterInstance
        {
            public IMonsterStaticData StaticData => default!;
            public MonsterGender Gender => default!;
            public IAbilityInstance Ability => default!;
            public byte Nature => default!;
            public long Experience => default!;
            public short CurrentHP => default!;
            public byte StatusEffect => default!;
            public byte Friendship => default!;
            public bool IsShiny => default!;
            public int EggSteps => default!;
            public IItemInstance? HeldItem => default!;
            public IDictionary<string, string> Metadata => default!;
        }

        public Task<IMonsterInstance> GetByDataAsync(string monsterData, CancellationToken ct) => Task.FromResult<IMonsterInstance>(new NopMonsterInstance());
    }
}