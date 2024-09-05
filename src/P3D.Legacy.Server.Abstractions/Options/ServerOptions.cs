using FluentValidation;

namespace P3D.Legacy.Server.Abstractions.Options
{
    public sealed class ServerOptionsValidator : AbstractValidator<ServerOptions>
    {
        public ServerOptionsValidator()
        {
            RuleFor(static x => x.Name).NotEmpty();
            RuleFor(static x => x.Message).NotEmpty();
            RuleFor(static x => x.MaxPlayers).NotEmpty();
        }
    }

    public sealed record ServerOptions
    {
        public required string Name { get; init; } = default!;
        public required string Message { get; init; } = default!;
        public required int MaxPlayers { get; init; } = default!;
        public required bool OfflineEnabled { get; init; } = default!;
        public required bool ValidationEnabled { get; init; } = default!;
    }
}