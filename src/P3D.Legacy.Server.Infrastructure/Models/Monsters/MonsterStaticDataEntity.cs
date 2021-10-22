using P3D.Legacy.Common.Monsters;

using System.Collections.Generic;

namespace P3D.Legacy.Server.Infrastructure.Models.Monsters
{
    public sealed record MonsterStaticDataEntity(short Id, string Name, Stats BaseStats, byte BaseHappiness, AbilityContainer Abilities, float MaleRatio, int HatchCycles, bool IsBaby, ExperienceType ExperienceType, IReadOnlyList<IAttackStaticData> LearnableAttacks) : IMonsterStaticData;
}