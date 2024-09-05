using System;
using System.Collections.Generic;

namespace P3D.Legacy.Common.Monsters;

public interface IMonsterInstance
{
    IMonsterStaticData StaticData { get; }


    CatchInfo CatchInfo => CatchInfo.None;

    MonsterGender Gender { get; }

    IAbilityInstance Ability { get; }
    byte Nature { get; }

    long Experience { get; }
    byte Level => ExperienceCalculator.LevelForExperienceValue(StaticData.ExperienceType, Experience);

    Stats IV => Stats.None;
    Stats EV => Stats.None;

    short CurrentHP { get; }
    byte StatusEffect { get; }

    byte Friendship { get; }

    bool IsShiny { get; }

    int EggSteps { get; }

    IReadOnlyList<IAttackInstance> Attacks => ArraySegment<IAttackInstance>.Empty;

    IItemInstance? HeldItem { get; }

    IDictionary<string, string> Metadata { get; }

    string? ToString() => !string.IsNullOrWhiteSpace(CatchInfo.Nickname) ? CatchInfo.Nickname : StaticData.Name;
}