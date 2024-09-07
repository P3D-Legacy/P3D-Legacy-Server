using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Server.Domain.Commands.World;

public sealed record ChangeWorldWeatherCommand(WorldWeather Weather) : ICommand;