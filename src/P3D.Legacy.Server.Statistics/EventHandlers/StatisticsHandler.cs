using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.Infrastructure.Repositories.Statistics;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Statistics.EventHandlers
{
    internal sealed class StatisticsHandler :
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

        public async Task HandleAsync(IReceiveContext<PlayerTriggeredEventEvent> context, CancellationToken ct)
        {
            var (player, @event) = context.Message;
            await _statisticsRepository.IncrementActionAsync(player.Id, $"event_{@event.EventType}", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentGlobalMessageEvent> context, CancellationToken ct)
        {
            var player = context.Message.Player;
            await _statisticsRepository.IncrementActionAsync(player.Id, "message_global", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentLocalMessageEvent> context, CancellationToken ct)
        {
            var player = context.Message.Player;
            await _statisticsRepository.IncrementActionAsync(player.Id, "message_local", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentPrivateMessageEvent> context, CancellationToken ct)
        {
            var player = context.Message.Player;
            await _statisticsRepository.IncrementActionAsync(player.Id, "message_private", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerSentCommandEvent> context, CancellationToken ct)
        {
            var player = context.Message.Player;

            await _statisticsRepository.IncrementActionAsync(player.Id, "message_command", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerJoinedEvent> context, CancellationToken ct)
        {
            var player = context.Message.Player;

            await _statisticsRepository.IncrementActionAsync(player.Id, "player_joined", ct);
        }

        public async Task HandleAsync(IReceiveContext<PlayerLeftEvent> context, CancellationToken ct)
        {
            var (id, _, _) = context.Message;

            await _statisticsRepository.IncrementActionAsync(id, "player_left", ct);
        }

        public async Task HandleAsync(IReceiveContext<WorldUpdatedEvent> context, CancellationToken ct)
        {
            var (state, oldState) = context.Message;

            if (state.Season != oldState.Season)
            {
                await _statisticsRepository.IncrementActionAsync("world_update_season", ct);
            }
            if (state.Weather != oldState.Weather)
            {
                await _statisticsRepository.IncrementActionAsync("world_update_weather", ct);
            }
            if (state.Time != oldState.Time)
            {
                await _statisticsRepository.IncrementActionAsync("world_update_time", ct);
            }
        }
    }
}