namespace P3D.Legacy.Server.Infrastructure.Options
{
    public record PokeAPIOptions
    {
        public string GraphQLEndpoint { get; init; } = default!;
    }
}