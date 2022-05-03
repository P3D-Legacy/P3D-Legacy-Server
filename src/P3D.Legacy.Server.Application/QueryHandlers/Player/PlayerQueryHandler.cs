using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    public class PlayerQueryHandler :
        IQueryHandler<GetPlayersInitializedQuery, ImmutableArray<IPlayer>>,
        IQueryHandler<GetPlayerViewModelsPaginatedQuery, (long Count, ImmutableArray<PlayerViewModel> Models)>,
        IQueryHandler<GetPlayerViewModelsQuery, ImmutableArray<PlayerViewModel>>,
        IQueryHandler<GetPlayerViewModelQuery, PlayerViewModel?>
    {
        private readonly IPlayerContainerReader _playerContainer;

        public PlayerQueryHandler(IPlayerContainerReader playerContainer)
        {
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<ImmutableArray<IPlayer>> HandleAsync(GetPlayersInitializedQuery query, CancellationToken ct)
        {
            return await _playerContainer.GetAllAsync(ct).AreInitializedAsync().ToImmutableArrayAsync(ct);
        }

        public async Task<(long Count, ImmutableArray<PlayerViewModel> Models)> HandleAsync(GetPlayerViewModelsPaginatedQuery query, CancellationToken ct)
        {
            var (skip, take) = query;

            var baseQuery = _playerContainer.GetAllAsync(ct).AreInitializedAsync();
            return (
                await baseQuery.CountAsync(ct),
                await baseQuery.Skip(skip).Take(take).Select(x => new PlayerViewModel(x.Origin, x.Name, x.GameJoltId)).ToImmutableArrayAsync(ct)
            );
        }


        public async Task<ImmutableArray<PlayerViewModel>> HandleAsync(GetPlayerViewModelsQuery query, CancellationToken ct)
        {
            return await _playerContainer.GetAllAsync(ct).AreInitializedAsync().Select(x => new PlayerViewModel(x.Origin, x.Name, x.GameJoltId)).ToImmutableArrayAsync(ct);
        }

        public async Task<PlayerViewModel?> HandleAsync(GetPlayerViewModelQuery query, CancellationToken ct)
        {
            var origin = query.Origin;

            return await _playerContainer.GetAsync(Origin.FromNumber(origin), ct) is { Permissions: > PermissionTypes.UnVerified } x
                ? new PlayerViewModel(x.Origin, x.Name, x.GameJoltId)
                : null;
        }
    }
}