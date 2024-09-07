using P3D.Legacy.Common;
using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Services;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services;

internal class DefaultPlayerContainer : IPlayerContainerWriterAsync, IPlayerContainerReader
{
    private class PlayerEqualityComparer : IEqualityComparer<IPlayer>
    {
        public bool Equals(IPlayer? x, IPlayer? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.ConnectionId, y.ConnectionId, StringComparison.Ordinal);
        }

        public int GetHashCode(IPlayer obj) => obj.ConnectionId.GetHashCode(StringComparison.Ordinal);
    }

    private ImmutableHashSet<IPlayer> _connections = ImmutableHashSet.Create<IPlayer>().WithComparer(new PlayerEqualityComparer());

    public IPlayer? Get(Origin origin) => _connections.FirstOrDefault(x => x.Origin == origin);
    public IEnumerable<IPlayer> GetAll() => _connections;

    public Task AddAsync(IPlayer player, CancellationToken ct)
    {
        _connections = _connections.Add(player);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(IPlayer player, CancellationToken ct)
    {
        var oldConnections = _connections;
        _connections = _connections.Remove(player);
        return Task.FromResult(!ReferenceEquals(oldConnections, _connections));
    }
}