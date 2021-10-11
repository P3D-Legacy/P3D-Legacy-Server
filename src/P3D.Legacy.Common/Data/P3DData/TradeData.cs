using System;

namespace P3D.Legacy.Common.Data.P3DData
{
    public sealed record TradeData : P3DData
    {
        public MonsterData Monster { get; }

        public TradeData(in ReadOnlySpan<char> data) : base(in data)
        {
            Monster = new MonsterData(new DataItemStorage(data.ToString()));
        }

        public void Deconstruct(out MonsterData monster)
        {
            monster = Monster;
        }

        public override string ToP3DString()
        {
            throw new NotImplementedException();
        }
    }
}