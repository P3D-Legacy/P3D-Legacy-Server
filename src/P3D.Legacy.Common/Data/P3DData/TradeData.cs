using System;

namespace P3D.Legacy.Common.Data.P3DData
{
    public sealed record TradeData : P3DData
    {
        public DataItemStorage MonsterData { get; }

        public TradeData(in ReadOnlySpan<char> data) : base(in data)
        {
            MonsterData = new DataItemStorage(data.ToString());
        }

        public void Deconstruct(out DataItemStorage monsterData)
        {
            monsterData = MonsterData;
        }

        public override string ToP3DString()
        {
            throw new NotImplementedException();
        }
    }
}