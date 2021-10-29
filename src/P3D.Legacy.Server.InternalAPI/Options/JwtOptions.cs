namespace P3D.Legacy.Server.InternalAPI.Options
{
    public record JwtOptions
    {
        public string SignKey { get; init; } = default!;
        public string EncryptionKey { get; init; } = default!;
    }
}