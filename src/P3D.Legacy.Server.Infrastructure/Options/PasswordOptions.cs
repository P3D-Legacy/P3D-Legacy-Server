using FluentValidation;

namespace P3D.Legacy.Server.Infrastructure.Options;

public sealed class PasswordOptionsValidator : AbstractValidator<PasswordOptions>
{
    public PasswordOptionsValidator()
    {
        RuleFor(static x => x.RequiredLength).NotEmpty();
        RuleFor(static x => x.RequiredUniqueChars).NotEmpty();
    }
}

public sealed record PasswordOptions
{
    public required int RequiredLength { get; init; } = 6;

    public required int RequiredUniqueChars { get; init; } = 1;

    public required bool RequireNonAlphanumeric { get; init; } = true;

    public required bool RequireLowercase { get; init; } = true;

    public required bool RequireUppercase { get; init; } = true;

    public required bool RequireDigit { get; init; } = true;
}