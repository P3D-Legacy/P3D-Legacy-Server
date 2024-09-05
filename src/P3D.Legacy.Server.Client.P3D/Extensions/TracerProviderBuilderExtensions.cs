using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.Client.P3D.Extensions;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddClientP3DInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource("P3D.Legacy.Server.Client.P3D");

        return builder;
    }
}