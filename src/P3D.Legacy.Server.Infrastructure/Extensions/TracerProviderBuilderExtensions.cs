using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.Infrastructure.Extensions;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddInfrastructureInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource("P3D.Legacy.Server.Infrastructure");

        return builder;
    }
}