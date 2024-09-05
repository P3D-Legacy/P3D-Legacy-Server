using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Infrastructure.Models.Statistics;

public sealed record StatisticsEntity(PlayerId Id, string Action, int Count);