using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.CQERS.Extensions
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddCQERSInstrumentation(this TracerProviderBuilder builder)
        {
            builder.AddSource("P3D.Legacy.Server.CQERS");

            return builder;
        }
    }
}