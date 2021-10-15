namespace P3D.Legacy.Server.Options
{
    public sealed record P3DOptions
    {
        public string APIEndpointV1 { get; init; } = default!;
        public string APIToken { get; init; } = default!;
    }
}