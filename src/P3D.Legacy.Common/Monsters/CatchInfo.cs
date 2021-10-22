namespace P3D.Legacy.Common.Monsters
{
    public sealed record CatchInfo
    {
        public static CatchInfo None => new();

        public string Full => $"{Method} {Location}";
        public string Method { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;

        public string TrainerName { get; init; } = string.Empty;
        public uint? TrainerId { get; init; }

        public byte ContainerId { get; init; }

        public string? Nickname { get; init; }
    }
}