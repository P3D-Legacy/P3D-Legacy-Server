using FluentValidation;

using System.Security.Cryptography;

namespace P3D.Legacy.Server.InternalAPI.Options
{
    public sealed class JwtOptionsValidator : AbstractValidator<JwtOptions>
    {
        public JwtOptionsValidator()
        {
            RuleFor(x => x.RsaPrivateKey).MinimumLength(16).When(x => !string.IsNullOrEmpty(x.RsaPrivateKey));
        }
    }

    public record JwtOptions
    {
        public string RsaPrivateKey { get; init; } = default!;


        private RSA? _rsa; // TODO: Cache or create new?
        public RSA GetKey()
        {
            if (_rsa is null)
            {
                _rsa = RSA.Create();
                if (!string.IsNullOrEmpty(RsaPrivateKey))
                    _rsa.ImportFromPem(RsaPrivateKey);
            }
            return _rsa;
        }
    }
}