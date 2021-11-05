namespace P3D.Legacy.Server.InternalAPI.Options
{
    public record JwtOptions
    {
        public string RsaPrivateKey { get; init; } = default!;
        public string RsaPublicKey { get; init; } = default!;
    }
}