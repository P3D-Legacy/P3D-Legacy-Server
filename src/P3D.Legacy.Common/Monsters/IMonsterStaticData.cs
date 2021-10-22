using System.Collections.Generic;

namespace P3D.Legacy.Common.Monsters
{
    public interface IMonsterStaticData
    {
        short Id { get; }
        string Name { get; }

        Stats BaseStats { get; }
        byte BaseHappiness { get; }

        AbilityContainer Abilities { get; }

        float MaleRatio { get; }

        int HatchCycles { get; }

        bool IsBaby { get; }

        ExperienceType ExperienceType { get; }

        IReadOnlyList<IAttackStaticData> LearnableAttacks { get; }

        string? ToString() => $"{Name}";
    }
}