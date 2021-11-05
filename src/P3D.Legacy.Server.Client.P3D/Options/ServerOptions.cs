namespace P3D.Legacy.Server.Client.P3D.Options
{
    public sealed record P3DServerOptions
    {
        public string IP { get; init; } = default!;
        public ushort Port { get; init; } = default!;
    }
}