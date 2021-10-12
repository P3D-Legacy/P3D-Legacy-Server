using System;
using System.Collections.Generic;

namespace P3D.Legacy.Common.Data.P3DData
{
    public sealed record BattleHostData : P3DData
    {
        private static IReadOnlyList<string> ParseHostData(ReadOnlySpan<char> data)
        {
            var newQueries = new List<string>();
            var tempData = string.Empty;

            while (data.Length > 0)
            {
                if (data[0] == '|' && tempData[^1] == '}')
                {
                    newQueries.Add(tempData);
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
                newQueries.Add(tempData);
            }

            return newQueries;
        }

        public IReadOnlyList<string> Queries { get; }

        public BattleHostData(in ReadOnlySpan<char> data) : base(in data)
        {
            Queries = ParseHostData(data);
        }

        public override string ToP3DString()
        {
            throw new NotImplementedException();
        }
    }
}