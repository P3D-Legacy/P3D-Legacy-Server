using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.CommunicationAPI.Extensions;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddCommunicationAPIInstrumentation(this TracerProviderBuilder builder)
    {
            builder.AddSource("P3D.Legacy.Server.CommunicationAPI");

            return builder;
        }
}