using P3D.Legacy.Common;
using P3D.Legacy.Common.Monsters;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Monsters
{
    public class NopMonsterRepository : IMonsterRepository
    {
        private sealed record NopMonsterInstance : IMonsterInstance
        {
            public IMonsterStaticData StaticData { get; } = default!;
            public MonsterGender Gender { get; } = default!;
            public IAbilityInstance Ability { get; } = default!;
            public byte Nature { get; } = default!;
            public long Experience { get; } = default!;
            public short CurrentHP { get; } = default!;
            public byte StatusEffect { get; } = default!;
            public byte Friendship { get; } = default!;
            public bool IsShiny { get; } = default!;
            public int EggSteps { get; } = default!;
            public IItemInstance? HeldItem { get; } = default!;
            public IDictionary<string, string> Metadata { get; } = default!;
        }

        public Task<IMonsterInstance> GetByDataAsync(DataItemStorage dataItems) => Task.FromResult(new NopMonsterInstance() as IMonsterInstance);
    }
}