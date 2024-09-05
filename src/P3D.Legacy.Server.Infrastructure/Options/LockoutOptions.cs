using FluentValidation;

using System;

namespace P3D.Legacy.Server.Infrastructure.Options
{
    public sealed class LockoutOptionsValidator : AbstractValidator<LockoutOptions>
    {
        public LockoutOptionsValidator()
        {
            RuleFor(static x => x.MaxFailedAccessAttempts).NotEmpty();
            RuleFor(static x => x.DefaultLockoutTimeSpan).NotEmpty();
        }
    }

    public sealed record LockoutOptions
    {
        public required int MaxFailedAccessAttempts { get; init; } = 5;

        public required TimeSpan DefaultLockoutTimeSpan { get; init; } = TimeSpan.FromMinutes(5);
    }
}