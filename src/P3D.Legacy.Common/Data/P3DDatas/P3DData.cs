using System;
using System.Collections.Generic;
using System.Text;

namespace P3D.Legacy.Common.Data.P3DDatas
{
    public abstract record P3DData
    {
        protected static IReadOnlyList<string> ParseSeparatedData(ReadOnlySpan<char> monsterDatas)
        {
            var monsters = new List<string>();

            while (monsterDatas.IndexOf('|') is var entryIndex)
            {
                if (entryIndex == -1)
                {
                    monsters.Add(monsterDatas.ToString());
                    break;
                }

                var entry = monsterDatas.Slice(0, entryIndex);
                monsters.Add(entry.ToString());
                monsterDatas = monsterDatas.Slice(entryIndex + 1);
            }

            return monsters;
        }

        protected static string ToSeparatedData(IReadOnlyList<string> datas)
        {
            var sb = new StringBuilder();
            foreach (var data in datas)
            {
                if (sb.Length > 0)
                    sb.Append('|');

                sb.Append(data);
            }
            return sb.ToString();
        }

        protected P3DData(in ReadOnlySpan<char> data) { }

        public abstract string ToP3DString();
    }
}