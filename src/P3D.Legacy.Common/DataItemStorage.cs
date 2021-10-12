using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Common
{
    public record DataItemStorage : IReadOnlyCollection<string>
    {
        private static int BootlegCount(IReadOnlyDictionary<int, string> dataItems) => dataItems.Count > 0 ? dataItems.LastOrDefault().Key + 1 : 0;

        private static IEnumerable<string> Iterate(IReadOnlyDictionary<int, string> dataItems)
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
        public DataItemStorage(params string[] raw) => _dataItems = raw.Select(((x, i) => new KeyValuePair<int, string>(i, x))).ToDictionary(x => x.Key, x => x.Value);

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
        public char GetChar(int index) => Get(index) is { Length: >= 1 } str ? str[0] : '.';
        public bool GetBool(int index) => GetInt32(index) == 1;
        public int GetInt32(int index) => int.TryParse(Get(index), out var val) ? val : 0;
        public ulong GetUInt64(int index) => ulong.TryParse(Get(index), out var val) ? val : 0UL;

        public void Set(int index, in ReadOnlySpan<char> value) => _dataItems[index] = value.ToString();
        public void SetChar(int index, char value) => Set(index, value.ToString());
        public void SetBool(int index, bool value) => SetInt32(index, value ? 1 : 0);
        public void SetInt32(int index, int value) => Set(index, value.ToString());
        public void SetUInt64(int index, ulong value) => Set(index, value.ToString());

        public override string ToString() => string.Join("*", _dataItems);
    }
}