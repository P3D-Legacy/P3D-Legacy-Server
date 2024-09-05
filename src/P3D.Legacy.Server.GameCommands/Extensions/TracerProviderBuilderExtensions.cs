using OpenTelemetry.Trace;

namespace P3D.Legacy.Server.GameCommands.Extensions;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddGameCommandsInstrumentation(this TracerProviderBuilder builder)
    {
            builder.AddSource("P3D.Legacy.Server.GameCommands");

            return builder;
        }
}