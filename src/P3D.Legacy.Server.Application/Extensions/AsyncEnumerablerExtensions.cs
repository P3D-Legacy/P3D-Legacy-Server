using P3D.Legacy.Server.Domain;

using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Application.Extensions;

public static class AsyncEnumerablerExtensions
{
    public static IEnumerable<IPlayer> AreInitialized(this IEnumerable<IPlayer> query) => query.Where(static x => x.State == PlayerState.Initialized);
}