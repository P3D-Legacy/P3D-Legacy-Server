using System;
using System.Collections.Generic;
using System.Text;

namespace P3D.Legacy.Common.Extensions
{
    public static class DataItemsExtensions
    {
        public static IDictionary<string, string> MonsterDataToDictionary(this ReadOnlySpan<char> monsterData)
        {
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            while (monsterData.IndexOf('}') is var entryIndex && entryIndex != -1)
            {
                var entry = monsterData.Slice(0, entryIndex);
                monsterData = monsterData.Slice(entryIndex + 1);

                if (entry.IndexOf('"') is var idStartIndex && idStartIndex == -1)
                    continue;

                var idSpan = entry.Slice(idStartIndex + 1);
                if (idSpan.IndexOf('"') is var idEndIndex && idEndIndex == -1)
                    continue;

                idSpan = idSpan.Slice(0, idEndIndex);

                var valueIndex = idStartIndex + 1 + idEndIndex + 1;
                var key = idSpan.ToString();
                var value = entry[(valueIndex + 1)..^1].ToString();
                dict.Add(key, value);
            }
            return dict;
        }

        public static string DictionaryToMonsterData(this IDictionary<string, string> dictionary)
        {
            var sb = new StringBuilder();
            foreach (var (key, value) in dictionary)
                sb.Append("{\"").Append(key).Append('"').Append('[').Append(value).Append("]}");
            return sb.ToString();
        }
    }
}