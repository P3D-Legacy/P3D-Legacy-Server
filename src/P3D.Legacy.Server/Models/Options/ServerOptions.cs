namespace P3D.Legacy.Server.Models.Options
{
    public sealed record ServerOptions
    {
        public string IP { get; init; } = default!;
        public ushort Port { get; init; } = default!;

        public string ServerName { get; init; } = default!;
        public string ServerMessage { get; init; } = default!;
        public int MaxPlayers { get; init; } = default!;
    }
}