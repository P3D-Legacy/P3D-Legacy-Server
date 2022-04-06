using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Players
{
    public class PlayerQueries : IPlayerQueries
    {
        private readonly IPlayerContainerReader _playerContainer;

        public PlayerQueries(IPlayerContainerReader playerContainer)
        {
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<PlayerViewModel?> GetAsync(long origin, CancellationToken ct)
        {
            return await _playerContainer.GetAsync(Origin.FromNumber(origin), ct) is { Permissions: > PermissionFlags.UnVerified } x ? new PlayerViewModel(x.Origin, x.Name, x.GameJoltId) : null;
        }

        public async Task<(long Count, IReadOnlyList<PlayerViewModel> Models)> GetAllAsync(int skip, int take, CancellationToken ct)
        {
            return (
                await _playerContainer.GetAllAsync(ct).Where(x => x.Permissions > PermissionFlags.UnVerified).CountAsync(ct),
                await _playerContainer.GetAllAsync(ct).Where(x => x.Permissions > PermissionFlags.UnVerified).Skip(skip).Take(take).Select(x => new PlayerViewModel(x.Origin, x.Name, x.GameJoltId)).ToListAsync(ct)
            );
        }

        public IAsyncEnumerable<PlayerViewModel> GetAllAsync(CancellationToken ct)
        {
            return _playerContainer.GetAllAsync(ct).Where(x => x.Permissions > PermissionFlags.UnVerified).Select(x => new PlayerViewModel(x.Origin, x.Name, x.GameJoltId));
        }
    }
}