using P3D.Legacy.Common;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace P3D.Legacy.Server.Client.P3D
{
    public sealed record DataItemStorage : IReadOnlyCollection<string>
    {
        private static int BootlegCount(Dictionary<int, string> dataItems) => dataItems.Count > 0 ? dataItems.LastOrDefault().Key + 1 : 0;

        private static IEnumerable<string> Iterate(Dictionary<int, string> dataItems)
        {
            var min = 0;
            var max = BootlegCount(dataItems);

            if (min == max)
                yield break;

            for (var i = min; i < max; i++)
            {
                yield return dataItems.TryGetValue(i, out var val) ? val : string.Empty;
            }
        }

        private readonly Dictionary<int, string> _dataItems = new();


        public int Count => BootlegCount(_dataItems);

        public DataItemStorage() : this(Array.Empty<string>()) { }
        public DataItemStorage(params string[] raw)
        {
            _dataItems = raw.Select(static (x, i) => new KeyValuePair<int, string>(i, x)).ToDictionary(static x => x.Key, static x => x.Value);
        }

        public IEnumerator<string> GetEnumerator() => Iterate(_dataItems).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Iterate(_dataItems).GetEnumerator();

        public void Add(DataItemStorage dataItemStorage)
        {
            foreach (var dataItem in dataItemStorage)
            {
                Set(Count, dataItem);
            }
        }

        public string Get(int index) => !_dataItems.TryGetValue(index, out var value) ? string.Empty : value;
        public char? GetChar(int index) => Get(index) is { Length: >= 1 } str ? str[0] : null;
        public bool GetBool(int index) => GetInt32(index) == 1;
        public int GetInt32(int index) => int.TryParse(Get(index), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : 0;
        public ulong GetUInt64(int index) => ulong.TryParse(Get(index), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : 0UL;
        public long GetInt64(int index) => long.TryParse(Get(index), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : 0L;
        public Origin GetOrigin(int index) => Origin.From(GetInt64(index));

        public void Set(int index, ImmutableArray<string> value)
        {
            for (var i = 0; i < value.Length; i++)
                Set(index + i, value[i]);
        }
        public void Set(int index, in ReadOnlySpan<char> value) => _dataItems[index] = value.ToString();
        public void Set(int index, char? value) => Set(index, value?.ToString(CultureInfo.InvariantCulture) ?? ".");
        public void Set(int index, bool value) => Set(index, value ? 1 : 0);
        public void Set(int index, int value) => Set(index, value.ToString(CultureInfo.InvariantCulture));
        public void Set(int index, ulong value) => Set(index, value.ToString(CultureInfo.InvariantCulture));
        public void Set(int index, long value) => Set(index, value.ToString(CultureInfo.InvariantCulture));
        public void Set(int index, Origin value) => Set(index, (long) value);

        public override string ToString() => string.Join('*', _dataItems.Values);
    }
}