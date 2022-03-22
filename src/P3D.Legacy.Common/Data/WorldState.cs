using System;

namespace P3D.Legacy.Common.Data
{
    public record WorldState(TimeSpan Time, WorldSeason Season, WorldWeather Weather);
}