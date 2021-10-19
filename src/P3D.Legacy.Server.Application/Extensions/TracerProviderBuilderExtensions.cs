using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.Application.Extensions
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddApplicationInstrumentation(this TracerProviderBuilder builder)
        {
            builder.AddSource("P3D.Legacy.Server.Application");

            return builder;
        }
    }
}