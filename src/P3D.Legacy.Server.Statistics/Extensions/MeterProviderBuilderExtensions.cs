using OpenTelemetry.Metrics;

namespace P3D.Legacy.Server.Statistics.Extensions;

public static class MeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddStatisticsInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter("P3D.Legacy.Server.Statistics");

        return builder;
    }
}