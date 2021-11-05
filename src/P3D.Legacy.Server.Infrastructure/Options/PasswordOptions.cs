namespace P3D.Legacy.Server.Infrastructure.Options
{
    public record PasswordOptions
    {
        public int RequiredLength { get; init; } = 6;

        public int RequiredUniqueChars { get; init; } = 1;

        public bool RequireNonAlphanumeric { get; init; } = true;

        public bool RequireLowercase { get; init; } = true;

        public bool RequireUppercase { get; init; } = true;

        public bool RequireDigit { get; init; } = true;
    }
}