using FluentValidation;

namespace P3D.Legacy.Server.DiscordBot.Options;

public sealed class DiscordOptionsValidator : AbstractValidator<DiscordOptions>
{
    public DiscordOptionsValidator()
    {
        //RuleFor(x => x.BotToken).NotEmpty().NotInteger().NotBoolean();
        RuleFor(static x => x.PasstroughChannelId).NotEmpty().When(static x => !string.IsNullOrEmpty(x.BotToken));
    }
}

public sealed record DiscordOptions
{
    public required string BotToken { get; init; }
    public required ulong PasstroughChannelId { get; init; }
}