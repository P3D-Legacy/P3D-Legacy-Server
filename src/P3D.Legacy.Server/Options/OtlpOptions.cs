using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using OpenTelemetry.Exporter;

namespace P3D.Legacy.Server.Options;

public sealed class OtlpOptionsValidator : AbstractValidator<OtlpOptions>
{
    public OtlpOptionsValidator()
    {
        RuleFor(static x => x.LoggingEndpoint).IsUri().IsUrlTcpEndpointAvailable().When(static x => !string.IsNullOrEmpty(x.LoggingEndpoint));
        RuleFor(static x => x.TracingEndpoint).IsUri().IsUrlTcpEndpointAvailable().When(static x => !string.IsNullOrEmpty(x.TracingEndpoint));
        RuleFor(static x => x.MetricsEndpoint).IsUri().IsUrlTcpEndpointAvailable().When(static x => !string.IsNullOrEmpty(x.MetricsEndpoint));
    }
}

public sealed record OtlpOptions
{
    public required string LoggingEndpoint { get; init; }
    public required OtlpExportProtocol LoggingProtocol { get; init; }
    public required string TracingEndpoint { get; init; }
    public required OtlpExportProtocol TracingProtocol { get; init; }
    public required string MetricsEndpoint { get; init; }
    public required OtlpExportProtocol MetricsProtocol { get; init; }
}