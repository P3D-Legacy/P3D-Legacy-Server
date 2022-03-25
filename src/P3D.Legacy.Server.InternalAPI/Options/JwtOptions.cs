using FluentValidation;

namespace P3D.Legacy.Server.InternalAPI.Options
{
    public sealed class JwtOptionsValidator : AbstractValidator<JwtOptions>
    {
        public JwtOptionsValidator()
        {
            RuleFor(options => options.RsaPrivateKey).MinimumLength(16);
            RuleFor(options => options.RsaPublicKey).MinimumLength(16);
        }
    }

    public record JwtOptions
    {
        public string RsaPrivateKey { get; init; } = default!;
        public string RsaPublicKey { get; init; } = default!;
    }
}