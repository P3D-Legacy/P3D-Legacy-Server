namespace P3D.Legacy.Server.Infrastructure.Options
{
    public record LiteDbOptions
    {
        public string Path { get; init; } = default!;
    }
}