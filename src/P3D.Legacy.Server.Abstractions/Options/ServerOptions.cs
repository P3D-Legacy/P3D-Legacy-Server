using FluentValidation;

namespace P3D.Legacy.Server.Abstractions.Options
{
    public sealed class ServerOptionsValidator : AbstractValidator<ServerOptions>
    {
        public ServerOptionsValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Message).NotEmpty();
            RuleFor(x => x.MaxPlayers).NotEmpty();
        }
    }

    public sealed record ServerOptions
    {
        public string Name { get; init; } = default!;
        public string Message { get; init; } = default!;
        public int MaxPlayers { get; init; } = default!;
        public bool ValidationEnabled { get; init; } = default!;
        public bool IsOfficial { get; init; } = default!;
    }
}