namespace P3D.Legacy.Server.Options
{
    public sealed record P3DOptions
    {
        public string APIEndpoint { get; init; } = default!;
        public string APIToken { get; init; } = default!;
    }
}