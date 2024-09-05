using System;

namespace P3D.Legacy.Server.Client.P3D.Data.P3DDatas;

public sealed record TradeData : P3DData
{
    public string MonsterData { get; }

    public TradeData(in ReadOnlySpan<char> data) : base(in data)
    {
        MonsterData = data.ToString();
    }

    public override string ToP3DString() => MonsterData;
}