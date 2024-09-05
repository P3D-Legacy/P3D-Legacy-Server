using FluentValidation;

using P3D.Legacy.Server.Infrastructure.Extensions;

namespace P3D.Legacy.Server.Infrastructure.Options
{
    public sealed class LiteDbOptionsValidator : AbstractValidator<LiteDbOptions>
    {
        public LiteDbOptionsValidator()
        {
            RuleFor(static x => x.ConnectionString).IsLiteDBConnectionString();
        }
    }

    public sealed record LiteDbOptions
    {
        public required string ConnectionString { get; init; } = default!;
    }
}