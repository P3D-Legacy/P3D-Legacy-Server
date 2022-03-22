using Microsoft.Extensions.Hosting;

using P3D.Legacy.Common.Data;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public class WorldService : BackgroundService
    {
        public WorldState State { get; private set; } = new(TimeSpan.Zero, WorldSeason.Spring, WorldWeather.Sunny);

        public WorldSeason Season { get => State.Season; set => State = State with { Season = value }; }
        public WorldWeather Weather { get => State.Weather; set => State = State with { Weather = value }; }
        public TimeSpan CurrentTime { get => State.Time; set => State = State with { Time = value }; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}