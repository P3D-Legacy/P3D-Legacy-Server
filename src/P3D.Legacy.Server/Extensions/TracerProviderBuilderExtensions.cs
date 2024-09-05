using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.Extensions;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddHostInstrumentation(this TracerProviderBuilder builder)
    {
            builder.AddSource("P3D.Legacy.Server.Host");

            return builder;
        }
}