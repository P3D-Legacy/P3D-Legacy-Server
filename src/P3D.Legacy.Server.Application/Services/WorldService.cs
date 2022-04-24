using ComposableAsync;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;

using RateLimiter;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public class WorldService : LongRunningBackgroundService
    {
        public WorldState State { get; private set; } = new(TimeSpan.Zero, WorldSeason.Spring, WorldWeather.Sunny);

        public WorldSeason Season { get => State.Season; set => State = State with { Season = value }; }
        public WorldWeather Weather { get => State.Weather; set => State = State with { Weather = value }; }
        public TimeSpan CurrentTime { get => State.Time; set => State = State with { Time = value }; }

        private readonly ILogger _logger;
        private readonly NotificationPublisher _notificationPublisher;
        private readonly TimeLimiter _checkTimeLimiter;

        public WorldService(ILogger<WorldService> logger, NotificationPublisher notificationPublisher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));

            var now = DateTime.UtcNow;
            var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, now.Kind);
            _checkTimeLimiter = TimeLimiter.GetPersistentTimeLimiter(1, TimeSpan.FromHours(1), _ => { }, new[] { currentHour });
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await _checkTimeLimiter;

                var now = DateTime.UtcNow;
                var weekOfYear = (int) (now.DayOfYear - ((now.DayOfWeek - DayOfWeek.Monday) / 7.0) + 1.0);
                var rand = new Random().Next(0, 100);

                var season = (weekOfYear % 4) switch
                {
                    0 => WorldSeason.Fall,
                    1 => WorldSeason.Winter,
                    2 => WorldSeason.Spring,
                    3 => WorldSeason.Summer,
                    _ => WorldSeason.Summer,
                };
                var weather = season switch
                {
                    WorldSeason.Fall => rand switch
                    {
                        //< 5 => WorldWeather.Snow,
                        >= 5 and < 80 => WorldWeather.Rain,
                        _ => WorldWeather.Clear,
                    },
                    WorldSeason.Winter => rand switch
                    {
                        < 20 => WorldWeather.Rain,
                        >= 20 and < 50 => WorldWeather.Clear,
                        //_ => WorldWeather.Snow,
                        _ => WorldWeather.Clear,
                    },
                    WorldSeason.Spring => rand switch
                    {
                        < 5 => WorldWeather.Sunny,
                        >= 5 and < 40 => WorldWeather.Rain,
                        _ => WorldWeather.Clear,
                    },
                    WorldSeason.Summer => rand switch
                    {
                        < 40 => WorldWeather.Clear,
                        >= 40 and < 80 => WorldWeather.Rain,
                        _ => WorldWeather.Sunny,
                    },
                    _ => WorldWeather.Clear,
                };

                var oldState = State;
                State = State with { Season = season, Weather = weather, Time = now.TimeOfDay };
                await _notificationPublisher.Publish(new WorldUpdatedNotification(State, oldState), ct);
            }
        }
    }
}