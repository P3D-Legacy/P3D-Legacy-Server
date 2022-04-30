using P3D.Legacy.Server.Abstractions;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Extensions
{
    public static class AsyncEnumerablerExtensions
    {
        public static IAsyncEnumerable<IPlayer> AreInitializedAsync(this IAsyncEnumerable<IPlayer> query) => query.Where(x => x.State == PlayerState.Initialized);

        public static async ValueTask<ImmutableArray<T>> ToImmutableArrayAsync<T>(this IAsyncEnumerable<T> source, CancellationToken ct = default)
        {
            var array = await source.ToArrayAsync(ct);
            return Unsafe.As<T[], ImmutableArray<T>>(ref array);
        }
    }
}