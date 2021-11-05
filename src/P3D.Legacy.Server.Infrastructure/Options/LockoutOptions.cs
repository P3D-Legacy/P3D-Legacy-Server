using System;

namespace P3D.Legacy.Server.Infrastructure.Options
{
    public record LockoutOptions
    {
        public int MaxFailedAccessAttempts { get; init; } = 5;

        public TimeSpan DefaultLockoutTimeSpan { get; init; } = TimeSpan.FromMinutes(5);
    }
}