using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

namespace P3D.Legacy.Server.Options
{
    public sealed class OtlpOptionsValidator : AbstractValidator<OtlpOptions>
    {
        public OtlpOptionsValidator()
        {
            RuleFor(static x => x.Host).IsUri().IsUrlTcpEndpointAvailable().When(static x => x.Enabled);
        }
    }

    public sealed record OtlpOptions
    {
        public bool Enabled { get; init; } = default!;
        public string Host { get; init; } = default!;
    }
}