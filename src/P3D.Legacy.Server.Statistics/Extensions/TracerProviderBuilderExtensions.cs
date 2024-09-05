using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.Statistics.Extensions;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddStatisticsInstrumentation(this TracerProviderBuilder builder)
    {
            builder.AddSource("P3D.Legacy.Server.Statistics");

            return builder;
        }
}