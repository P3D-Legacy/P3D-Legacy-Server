using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions.Extensions;

public static class AsyncEnumerablerExtensions
{
    public static async Task<ImmutableArray<T>> ToImmutableArrayAsync<T>(this IAsyncEnumerable<T> source, CancellationToken ct = default)
    {
            var array = await source.ToArrayAsync(ct);
            return Unsafe.As<T[], ImmutableArray<T>>(ref array);
        }
}