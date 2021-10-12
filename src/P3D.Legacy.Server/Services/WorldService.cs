using Microsoft.Extensions.Hosting;

using P3D.Legacy.Common.Data;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public class WorldService : BackgroundService
    {
        public WorldSeason Season { get; private set; } = WorldSeason.Spring;
        public WorldWeather Weather { get; private set; } = WorldWeather.Sunny;
        public TimeSpan CurrentTime { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

        }
    }
}