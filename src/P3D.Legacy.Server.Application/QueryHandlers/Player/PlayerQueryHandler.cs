using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Queries;
using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.Application.Services;

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

        public async Task<ImmutableArray<IPlayer>> Handle(GetPlayersInitializedQuery request, CancellationToken ct)
        {
            return await _playerContainer.GetAllAsync(ct).AreInitializedAsync().ToImmutableArrayAsync(ct);
        }

        public async Task<(long Count, ImmutableArray<PlayerViewModel> Models)> Handle(GetPlayerViewModelsPaginatedQuery request, CancellationToken ct) =>
        (
            await _playerContainer.GetAllAsync(ct).AreInitializedAsync().CountAsync(ct),
            await _playerContainer.GetAllAsync(ct).AreInitializedAsync().Skip(request.Skip).Take(request.Take).Select(x => new PlayerViewModel(x.Origin, x.Name, x.GameJoltId)).ToImmutableArrayAsync(ct)
        );


        public async Task<ImmutableArray<PlayerViewModel>> Handle(GetPlayerViewModelsQuery request, CancellationToken ct)
        {
            return await _playerContainer.GetAllAsync(ct).AreInitializedAsync().Select(x => new PlayerViewModel(x.Origin, x.Name, x.GameJoltId)).ToImmutableArrayAsync(ct);
        }

        public async Task<PlayerViewModel?> Handle(GetPlayerViewModelQuery request, CancellationToken ct)
        {
            return await _playerContainer.GetAsync(Origin.FromNumber(request.Origin), ct) is { Permissions: > PermissionTypes.UnVerified } x
                ? new PlayerViewModel(x.Origin, x.Name, x.GameJoltId)
                : null;
        }
    }
}