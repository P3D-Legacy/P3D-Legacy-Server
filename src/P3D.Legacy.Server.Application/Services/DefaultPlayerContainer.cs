using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    internal class DefaultPlayerContainer : IPlayerContainerWriter, IPlayerContainerReader
    {
        private class PlayerEqualityComparer : IEqualityComparer<IPlayer>
        {
            public bool Equals(IPlayer? x, IPlayer? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.ConnectionId == y.ConnectionId;
            }

            public int GetHashCode(IPlayer obj) => obj.ConnectionId.GetHashCode();
        }

        private readonly ILogger _logger;
        private ImmutableHashSet<IPlayer> _connections = ImmutableHashSet.Create<IPlayer>().WithComparer(new PlayerEqualityComparer());

        public DefaultPlayerContainer(ILogger<DefaultPlayerContainer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<IPlayer?> GetAsync(Origin id, CancellationToken ct) => Task.FromResult(_connections.FirstOrDefault(x => x.Id == id));
        public IAsyncEnumerable<IPlayer> GetAllAsync(CancellationToken ct) => _connections.ToAsyncEnumerable();
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
}