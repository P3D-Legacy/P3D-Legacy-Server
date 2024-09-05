using System;

namespace P3D.Legacy.Common.Data;

public sealed record WorldState(TimeSpan Time, WorldSeason Season, WorldWeather Weather);