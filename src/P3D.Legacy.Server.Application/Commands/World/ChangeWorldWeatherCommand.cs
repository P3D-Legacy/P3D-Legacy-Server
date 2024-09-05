using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.CQERS.Commands;

namespace P3D.Legacy.Server.Application.Commands.World;

public sealed record ChangeWorldWeatherCommand(WorldWeather Weather) : ICommand;