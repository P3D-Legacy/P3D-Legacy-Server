using System;
using System.Collections.Generic;

namespace P3D.Legacy.Common.Data.P3DData
{
    // If LeadMonsterIndex is not null, this is a confirmation, else client just gives the monsters for the battle
    public sealed record BattleOfferData : P3DData
    {
        private static IReadOnlyList<MonsterData> ParseOfferData(ReadOnlySpan<char> data)
        {
            var monsters = new List<MonsterData>();
            var tempData = string.Empty;

            while (data.Length > 0)
            {
                if (data[0] == '|' && tempData[^1] == '}')
                {
                    monsters.Add(new MonsterData(new DataItemStorage(tempData)));
                    tempData = "";
                }
                else
                {
                    tempData += data[0].ToString();
                }
                data = data.Slice(1);
            }
            if (tempData.StartsWith("{") && tempData.EndsWith("}"))
            {
                monsters.Add(new MonsterData(new DataItemStorage(tempData)));
            }

            return monsters;
        }

        public int? LeadMonsterIndex { get; }
        public IReadOnlyList<MonsterData> Monsters { get; }

        public BattleOfferData(in ReadOnlySpan<char> data) : base(in data)
        {
            LeadMonsterIndex = int.TryParse(data, out var index) ? (int?) index : null;
            Monsters = ParseOfferData(data);
        }

        public override string ToP3DString()
        {
            throw new NotImplementedException();
        }
    }
}