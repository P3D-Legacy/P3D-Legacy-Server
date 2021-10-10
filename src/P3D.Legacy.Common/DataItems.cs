using System;
using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Common
{
    public class DataItems
    {
        public static implicit operator string[](DataItems dataItems) => dataItems._dataItems;
        public static implicit operator DataItems(string dataItems) => new(dataItems);
        public static implicit operator DataItems(string[] dataItems) => new DataItems(dataItems);
        public static implicit operator DataItems(P3DData dataItems) => new DataItems(dataItems);


        public int Length => _dataItems.Length;

        private string[] _dataItems;

        public DataItems(params string[] dataItems)
        {
            if (dataItems == null)
                dataItems = Array.Empty<string>();

            _dataItems = dataItems;
        }


        public string this[int index]
        {
            get
            {
                if (_dataItems.Length < index + 1)
                    return string.Empty;

                // if string is null, make it empty.
                return _dataItems[index] ?? (_dataItems[index] = string.Empty);
            }

            set
            {
                if (_dataItems.Length < index + 1)
                    Array.Resize(ref _dataItems, index + 1);

                // if string is null, make it empty.
                if (value == null)
                    value = string.Empty;

                _dataItems[index] = value;
            }
        }


        public void AddToEnd(ReadOnlySpan<char> data)
        {
            var index = _dataItems.Length;

            if (_dataItems.Length < index + 1)
                Array.Resize(ref _dataItems, index + 1);

            _dataItems[index] = data.ToString();
        }


        public string[] ToArray() => _dataItems;

        public override string ToString() => string.Join("*", _dataItems);
    }
}