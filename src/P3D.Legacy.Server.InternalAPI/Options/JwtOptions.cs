using FluentValidation;

using System;
using System.Security.Cryptography;

namespace P3D.Legacy.Server.InternalAPI.Options
{
    public sealed class JwtOptionsValidator : AbstractValidator<JwtOptions>
    {
        public JwtOptionsValidator()
        {
            RuleFor(static x => x.RsaPrivateKey).MinimumLength(16).When(static x => !string.IsNullOrEmpty(x.RsaPrivateKey));
        }
    }

    public sealed record JwtOptions : IDisposable
    {
        public string RsaPrivateKey { get; init; } = default!;

        private RSA? _rsa;
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

        public void Dispose()
        {
            _rsa?.Dispose();
        }
    }
}