using FluentValidation;

using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.Extensions;

namespace P3D.Legacy.Server.Options
{
    public sealed class OtlpOptionsValidator : AbstractValidator<OtlpOptions>
    {
        public OtlpOptionsValidator()
        {
            RuleFor(x => x.Host).IsIPEndPoint().IsGrpcAvailable().When(x => x.Enabled);
        }
    }

    public sealed record OtlpOptions
    {
        public bool Enabled { get; init; } = default!;
        public string Host { get; init; } = default!;
    }
}