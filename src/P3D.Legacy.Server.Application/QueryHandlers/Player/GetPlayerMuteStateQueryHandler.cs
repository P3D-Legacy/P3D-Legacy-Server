using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Queries;
using P3D.Legacy.Server.Domain.Queries.Player;
using P3D.Legacy.Server.Domain.Repositories;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Player;

internal sealed class GetPlayerMuteStateQueryHandler : IQueryHandler<GetPlayerMuteStateQuery, bool>
{
    private readonly ILogger _logger;
    private readonly IMuteRepository _muteRepository;

    public GetPlayerMuteStateQueryHandler(ILogger<GetPlayerMuteStateQueryHandler> logger, IMuteRepository muteRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _muteRepository = muteRepository ?? throw new ArgumentNullException(nameof(muteRepository));
    }

    public async Task<bool> HandleAsync(GetPlayerMuteStateQuery query, CancellationToken ct)
    {
        var (playerId, targetPlayerId) = query;

        return await _muteRepository.IsMutedAsync(playerId, targetPlayerId, ct);
    }
}