using System;
using System.Collections.Generic;
using System.Globalization;

namespace P3D.Legacy.Server.Client.P3D.Data.P3DDatas;

// If LeadMonsterIndex is not null, this is a confirmation, else client just gives the monsters for the battle
public sealed record BattleOfferData : P3DData
{
    public int? LeadMonsterIndex { get; }
    public IReadOnlyList<string> MonsterDatas { get; }

    public BattleOfferData(in ReadOnlySpan<char> data) : base(in data)
    {
        LeadMonsterIndex = int.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) ? index : null;
        MonsterDatas = ParseSeparatedData(data);
    }

    public override string ToP3DString()
    {
        return LeadMonsterIndex is { } val ? val.ToString(CultureInfo.InvariantCulture) : ToSeparatedData(MonsterDatas);
    }
}