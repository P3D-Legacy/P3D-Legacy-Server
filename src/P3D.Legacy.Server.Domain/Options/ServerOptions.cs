using FluentValidation;

namespace P3D.Legacy.Server.Domain.Options;

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
    public string Name { get; set; } = default!;
    public string Message { get; set; } = default!;
    public int MaxPlayers { get; set; } = default!;
    public bool OfflineEnabled { get; set; } = default!;
    public bool ValidationEnabled { get; set; } = default!;
}