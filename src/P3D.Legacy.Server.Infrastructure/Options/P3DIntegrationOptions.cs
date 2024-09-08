using FluentValidation;

namespace P3D.Legacy.Server.Infrastructure.Options;

public sealed class P3DIntegrationOptionsValidator : AbstractValidator<P3DIntegrationOptions>
{
    public P3DIntegrationOptionsValidator() { }
}

public sealed record P3DIntegrationOptions
{
    public required bool IsOfficial { get; set; }
}