using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.Infrastructure.Repositories.Statistics;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.EventHandlers
{
    [SuppressMessage("Performance", "CA1812")]
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
}