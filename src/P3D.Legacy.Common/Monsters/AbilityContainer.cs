using System;
using System.Linq;
using System.Text;

namespace P3D.Legacy.Common.Monsters
{
    public class AbilityContainer
    {
        public IAbilityInstance First { get; } = default!;
        public IAbilityInstance? Second { get; } = default!;
        public IAbilityInstance? Hidden { get; } = default!;

        public AbilityContainer(params IAbilityInstance[] abilities)
        {
            abilities = abilities.OrderBy(static x => x.StaticData.Id).ThenBy(static x => !x.IsHidden).ToArray();

            if (abilities.Length > 0)
                First = abilities[0];

            if (abilities.Length > 1)
            {
                if (!abilities[1].IsHidden)
                    Second = abilities[1];
                else
                    Hidden = abilities[1];
            }

            if (abilities.Length > 2)
                Hidden = abilities[2];

            if (abilities.Length > 3)
                throw new ArgumentOutOfRangeException(nameof(abilities));
        }

        public override string ToString()
        {
            var output = new StringBuilder(First.ToString());
            if (Second is not null)
                output.Append($", ").Append(Second.ToString());
            if (Hidden is { })
                output.Append($", ").Append(Hidden.ToString());
            return output.ToString();
        }


        public bool Contains(IAbilityInstance ability) => First == ability || Second == ability || Hidden == ability;
        public bool Contains(short abilityId) => First.StaticData.Id == abilityId || Second?.StaticData.Id == abilityId || Hidden?.StaticData.Id == abilityId;
    }
}