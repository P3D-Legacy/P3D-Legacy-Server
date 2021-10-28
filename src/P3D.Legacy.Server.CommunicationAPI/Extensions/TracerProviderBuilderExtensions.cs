using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.CommunicationAPI.Extensions
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddDiscordAPIInstrumentation(this TracerProviderBuilder builder)
        {
            builder.AddSource("P3D.Legacy.Server.DiscordAPI");

            return builder;
        }
    }
}