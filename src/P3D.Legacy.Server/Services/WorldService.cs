using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Data;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public class WorldService
    {
        public WorldSeason Season { get; private set; } = WorldSeason.Spring;
        public WorldWeather Weather { get; private set; } = WorldWeather.Sunny;
        public TimeSpan CurrentTime { get; private set; }
    }
}