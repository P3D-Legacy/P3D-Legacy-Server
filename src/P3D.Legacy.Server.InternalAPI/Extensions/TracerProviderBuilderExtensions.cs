using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.InternalAPI.Extensions;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddInternalAPIInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource("P3D.Legacy.Server.InternalAPI");

        return builder;
    }
}