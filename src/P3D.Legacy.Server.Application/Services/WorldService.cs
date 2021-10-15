using Microsoft.Extensions.Hosting;

using P3D.Legacy.Common.Data;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public class WorldService : BackgroundService
    {
        public WorldSeason Season { get; set; } = WorldSeason.Spring;
        public WorldWeather Weather { get; set; } = WorldWeather.Sunny;
        public TimeSpan CurrentTime { get; set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

        }
    }
}