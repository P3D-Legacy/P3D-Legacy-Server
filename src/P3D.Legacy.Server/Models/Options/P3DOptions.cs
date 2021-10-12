namespace P3D.Legacy.Server.Models.Options
{
    public sealed record P3DOptions
    {
        public string APIEndpoint { get; init; } = default!;
        public string APIToken { get; init; } = default!;
    }
}