using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.DiscordBot.Extensions;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddDiscordBotInstrumentation(this TracerProviderBuilder builder)
    {
            builder.AddSource("P3D.Legacy.Server.DiscordBot");

            return builder;
        }
}