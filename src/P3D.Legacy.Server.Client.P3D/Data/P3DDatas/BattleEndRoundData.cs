using System;
using System.Collections.Generic;

namespace P3D.Legacy.Server.Client.P3D.Data.P3DDatas;

public sealed record BattleEndRoundData : P3DData
{
    public IReadOnlyList<string> Queries { get; }

    public BattleEndRoundData(in ReadOnlySpan<char> data) : base(in data)
    {
        Queries = ParseSeparatedData(data);
    }

    public override string ToP3DString() => ToSeparatedData(Queries);
}