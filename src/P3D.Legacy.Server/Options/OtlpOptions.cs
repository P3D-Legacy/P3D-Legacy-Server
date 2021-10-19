namespace P3D.Legacy.Server.Options
{
    public sealed record OtlpOptions
    {
        public bool Enabled { get; set; } = default!;
        public string Host { get; init; } = default!;
    }
}