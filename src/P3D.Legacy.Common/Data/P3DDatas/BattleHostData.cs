using System;
using System.Collections.Generic;

namespace P3D.Legacy.Common.Data.P3DDatas
{
    public sealed record BattleHostData : P3DData
    {
        public IReadOnlyList<string> Queries { get; }

        public BattleHostData(in ReadOnlySpan<char> data) : base(in data)
        {
            Queries = ParseSeparatedData(data);
        }

        public override string ToP3DString() => ToSeparatedData(Queries);
    }
}