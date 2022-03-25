using FluentValidation;

namespace P3D.Legacy.Server.InternalAPI.Options
{
    public sealed class JwtOptionsValidator : AbstractValidator<JwtOptions>
    {
        public JwtOptionsValidator()
        {
            RuleFor(x => x.RsaPrivateKey).MinimumLength(16);
            RuleFor(x => x.RsaPublicKey).MinimumLength(16);
        }
    }

    public record JwtOptions
    {
        public string RsaPrivateKey { get; init; } = default!;
        public string RsaPublicKey { get; init; } = default!;
    }
}