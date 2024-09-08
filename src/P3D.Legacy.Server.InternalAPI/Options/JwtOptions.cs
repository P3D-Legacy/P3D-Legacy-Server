using FluentValidation;

using System;
using System.Security.Cryptography;

namespace P3D.Legacy.Server.InternalAPI.Options;

public sealed class JwtOptionsValidator : AbstractValidator<JwtOptions>
{
    public JwtOptionsValidator()
    {
        RuleFor(static x => x.KeyType).NotEqual(KeyType.None);
        RuleFor(static x => x.PrivateKey).MinimumLength(16).When(static x => x.KeyType == KeyType.Rsa);
        RuleFor(static x => x.PrivateKey).MinimumLength(16).When(static x => x.KeyType == KeyType.ECDsa);
    }
}

public enum KeyType { None, Rsa, ECDsa }

public sealed record JwtOptions : IDisposable
{
    public required string PrivateKey { get; set; }
    public required KeyType KeyType { get; set; }

    private RSA? _rsa;

    public RSA GetRSAKey()
    {
        if (KeyType != KeyType.Rsa) throw new NotSupportedException();

        if (_rsa is null)
        {
            _rsa = RSA.Create();
            if (!string.IsNullOrEmpty(PrivateKey))
                _rsa.ImportFromPem(PrivateKey);
        }

        return _rsa;
    }

    private ECDsa? _ecdsa;

    public ECDsa GetECDsaKey()
    {
        if (KeyType != KeyType.ECDsa) throw new NotSupportedException();

        if (_ecdsa is null)
        {
            _ecdsa = ECDsa.Create();
            if (!string.IsNullOrEmpty(PrivateKey))
                _ecdsa.ImportFromPem(PrivateKey);
        }

        return _ecdsa;
    }

    public void Dispose()
    {
        _rsa?.Dispose();
        _ecdsa?.Dispose();
    }
}