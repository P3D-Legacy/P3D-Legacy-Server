using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace P3D.Legacy.Server.Client.P3D.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<ElementInfo<T>> WithMetadata<T>(this IEnumerable<T> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();
        var isFirst = true;
        var hasNext = enumerator.MoveNext();
        var index = 0;
        while (hasNext)
        {
            var current = enumerator.Current;
            hasNext = enumerator.MoveNext();
            yield return new ElementInfo<T>(index, current, isFirst, !hasNext);
            isFirst = false;
            index++;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct ElementInfo<T>(int Index, T Value, bool IsFirst, bool IsLast);
}