using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Events.Player;
using P3D.Legacy.Server.Domain.Repositories;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.EventHandlers;

internal sealed class StatisticsHandler :
    IEventHandler<PlayerUpdatedStateEvent>
{
    private readonly ILogger _logger;
    private readonly IStatisticsRepository _statisticsRepository;

    public StatisticsHandler(ILogger<StatisticsHandler> logger, IStatisticsRepository statisticsRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statisticsRepository = statisticsRepository ?? throw new ArgumentNullException(nameof(statisticsRepository));
    }

    public async Task HandleAsync(IReceiveContext<PlayerUpdatedStateEvent> context, CancellationToken ct)
    {
        var player = context.Message.Player;
        if (player is IP3DPlayerState)
        {
            await _statisticsRepository.IncrementActionAsync(player.Id, "player_update_state", ct);
        }
    }
}