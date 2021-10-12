using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace P3D.Legacy.Server.HttpLogging
{
    // Internal so we can change the requirements without breaking changes.
    internal sealed class HttpHeadersLogValue : IReadOnlyList<KeyValuePair<string, object>>
    {
        public enum Kind
        {
            Request,
            Response,
        }

        private readonly Kind _kind;
        private readonly Func<string, bool> _shouldRedactHeaderValue;

        private string? _formatted;
        private List<KeyValuePair<string, object>>? _values;

        public HttpHeaders Headers { get; }

        public HttpHeaders? ContentHeaders { get; }

        private List<KeyValuePair<string, object>> Values
        {
            get
            {
                if (_values == null)
                {
                    var values = new List<KeyValuePair<string, object>>();

                    foreach (var (key, value) in Headers)
                    {
                        values.Add(new KeyValuePair<string, object>(key, value));
                    }

                    if (ContentHeaders != null)
                    {
                        foreach (var (key, value) in ContentHeaders)
                        {
                            values.Add(new KeyValuePair<string, object>(key, value));
                        }
                    }

                    _values = values;
                }

                return _values;
            }
        }

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException(nameof(index));
                }

                return Values[index];
            }
        }

        public int Count => Values.Count;

        public HttpHeadersLogValue(Kind kind, HttpHeaders headers, HttpHeaders? contentHeaders, Func<string, bool> shouldRedactHeaderValue)
        {
            _kind = kind;
            _shouldRedactHeaderValue = shouldRedactHeaderValue;

            Headers = headers;
            ContentHeaders = contentHeaders;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();

        public override string ToString()
        {
            if (_formatted == null)
            {
                var builder = new StringBuilder();
                builder.AppendLine(_kind == Kind.Request ? "Request Headers:" : "Response Headers:");

                for (var i = 0; i < Values.Count; i++)
                {
                    var (key, value) = Values[i];
                    builder.Append(key);
                    builder.Append(": ");

                    if (_shouldRedactHeaderValue(key))
                    {
                        builder.Append('*');
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendJoin(", ", (IEnumerable<object>) value);
                        builder.AppendLine();
                    }
                }

                _formatted = builder.ToString();
            }

            return _formatted;
        }
    }
}