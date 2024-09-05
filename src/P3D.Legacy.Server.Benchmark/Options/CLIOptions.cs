namespace P3D.Legacy.Server.Benchmark.Options;

public sealed record CLIOptions
{
    public string Host { get; set; } = default!;
    public ushort Port { get; set; } = default!;
    public string BenchmarkType { get; set; } = default!;
    public int Batch { get; set; } = default!;
}