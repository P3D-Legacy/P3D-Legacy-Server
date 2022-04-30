using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Queries;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.Infrastructure.Services.Mutes;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class GetPlayerMuteStateQueryHandler : IQueryHandler<GetPlayerMuteStateQuery, bool>
    {
        private readonly ILogger _logger;
        private readonly IMuteManager _muteManager;

        public GetPlayerMuteStateQueryHandler(ILogger<GetPlayerMuteStateQueryHandler> logger, IMuteManager muteManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _muteManager = muteManager ?? throw new ArgumentNullException(nameof(muteManager));
        }

        public async Task<bool> Handle(GetPlayerMuteStateQuery request, CancellationToken ct)
        {
            var (playerId, targetPlayerId) = request;

            return await _muteManager.IsMutedAsync(playerId, targetPlayerId, ct);
        }
    }
}