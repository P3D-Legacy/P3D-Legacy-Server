using System;
using System.Collections.Generic;

namespace P3D.Legacy.Common.Data.P3DData
{
    // If LeadMonsterIndex is not null, this is a confirmation, else client just gives the monsters for the battle
    public sealed record BattleOfferData : P3DData
    {
        public int? LeadMonsterIndex { get; }
        public IReadOnlyList<string> MonsterDatas { get; }

        public BattleOfferData(in ReadOnlySpan<char> data) : base(in data)
        {
            LeadMonsterIndex = int.TryParse(data, out var index) ? index : null;
            MonsterDatas = ParseSeparatedData(data);
        }

        public override string ToP3DString()
        {
            return LeadMonsterIndex is { } val ? val.ToString() : ToSeparatedData(MonsterDatas);
        }
    }
}