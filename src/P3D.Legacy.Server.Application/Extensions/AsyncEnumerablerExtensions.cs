using P3D.Legacy.Server.Abstractions;

using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Application.Extensions
{
    public static class AsyncEnumerablerExtensions
    {
        public static IAsyncEnumerable<IPlayer> AreInitializedAsync(this IAsyncEnumerable<IPlayer> query) => query.Where(static x => x.State == PlayerState.Initialized);
    }
}