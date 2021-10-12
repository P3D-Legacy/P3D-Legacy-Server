using P3D.Legacy.Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Queries.Players
{
    public class PlayerQueries : IPlayerQueries
    {
        private readonly IPlayerContainerReader _playerContainer;

        public PlayerQueries(IPlayerContainerReader playerContainer)
        {
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<PlayerViewModel?> GetAsync(ulong id, CancellationToken ct) => await _playerContainer.GetAsync(id, ct) is { } x ? new PlayerViewModel(x.Id, x.Name, x.GameJoltId) : null;
        public IAsyncEnumerable<PlayerViewModel> GetAllAsync(CancellationToken ct) => _playerContainer.GetAllAsync(ct).Select(x => new PlayerViewModel(x.Id, x.Name, x.GameJoltId));
    }
}