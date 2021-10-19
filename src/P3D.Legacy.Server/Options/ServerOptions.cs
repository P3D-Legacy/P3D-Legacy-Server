namespace P3D.Legacy.Server.Options
{
    public sealed record ServerOptions
    {
        public string IP { get; init; } = default!;
        public ushort Port { get; init; } = default!;

        public string Name { get; init; } = default!;
        public string Message { get; init; } = default!;
        public int MaxPlayers { get; init; } = default!;
    }
}