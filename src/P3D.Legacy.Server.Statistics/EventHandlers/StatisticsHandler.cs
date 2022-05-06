using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.Infrastructure.Repositories.Statistics;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Statistics.EventHandlers
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class StatisticsHandler :
        IEventHandler<PlayerUpdatedStateEvent>,
        IEventHandler<PlayerTriggeredEventEvent>,
        IEventHandler<PlayerSentGlobalMessageEvent>,
        IEventHandler<PlayerSentLocalMessageEvent>,
        IEventHandler<PlayerSentPrivateMessageEvent>,
        IEventHandler<PlayerSentCommandEvent>,
        IEventHandler<PlayerJoinedEvent>,
        IEventHandler<PlayerLeftEvent>,
        IEventHandler<WorldUpdatedEvent>
    {
        private readonly ILogger _logger;
        private readonly IStatisticsRepository _statisticsRepository;

        public StatisticsHandler(ILogger<StatisticsHandler> logger, IStatisticsRepository statisticsRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _statisticsRepository = statisticsRepository ?? throw new ArgumentNullException(nameof(statisticsRepository));
        }

        public async Task HandleAsync(PlayerUpdatedStateEvent notification, CancellationToken ct)
        {
            var player = notification.Player;
            if (player is IP3DPlayerState)
            {
                await _statisticsRepository.IncrementActionAsync(player.Id, "player_update_state", ct);
            }
        }

        public async Task HandleAsync(PlayerTriggeredEventEvent notification, CancellationToken ct)
        {
            await _statisticsRepository.IncrementActionAsync(notification.Player.Id, $"event_{notification.Event.EventType}", ct);
        }

        public async Task HandleAsync(PlayerSentGlobalMessageEvent notification, CancellationToken ct)
        {
            await _statisticsRepository.IncrementActionAsync(notification.Player.Id, "message_global", ct);
        }

        public async Task HandleAsync(PlayerSentLocalMessageEvent notification, CancellationToken ct)
        {
            await _statisticsRepository.IncrementActionAsync(notification.Player.Id, "message_local", ct);
        }

        public async Task HandleAsync(PlayerSentPrivateMessageEvent notification, CancellationToken ct)
        {
            await _statisticsRepository.IncrementActionAsync(notification.Player.Id, "message_private", ct);
        }

        public async Task HandleAsync(PlayerSentCommandEvent notification, CancellationToken ct)
        {
            await _statisticsRepository.IncrementActionAsync(notification.Player.Id, "message_command", ct);
        }

        public async Task HandleAsync(PlayerJoinedEvent notification, CancellationToken ct)
        {
            await _statisticsRepository.IncrementActionAsync(notification.Player.Id, "player_joined", ct);
        }

        public async Task HandleAsync(PlayerLeftEvent notification, CancellationToken ct)
        {
            await _statisticsRepository.IncrementActionAsync(notification.Id, "player_left", ct);
        }

        public async Task HandleAsync(WorldUpdatedEvent notification, CancellationToken ct)
        {
            if (notification.State.Season != notification.OldState.Season)
            {
                await _statisticsRepository.IncrementActionAsync("world_update_season", ct);
            }
            if (notification.State.Weather != notification.OldState.Weather)
            {
                await _statisticsRepository.IncrementActionAsync("world_update_weather", ct);
            }
            if (notification.State.Time != notification.OldState.Time)
            {
                await _statisticsRepository.IncrementActionAsync("world_update_time", ct);
            }
        }
    }
}