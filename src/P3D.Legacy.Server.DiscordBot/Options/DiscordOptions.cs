using FluentValidation;

namespace P3D.Legacy.Server.DiscordBot.Options
{
    internal sealed class DiscordOptionsValidator : AbstractValidator<DiscordOptions>
    {
        public DiscordOptionsValidator()
        {
            //RuleFor(options => options.BotToken).NotEmpty().NotInteger().NotBoolean();
            RuleFor(options => options.BotToken).NotEmpty();
            RuleFor(options => options.PasstroughChannelId).GreaterThan(0UL);
        }
    }

    internal sealed record DiscordOptions
    {
        public ulong PasstroughChannelId { get; init; } = default!;
        public string BotToken { get; init; } = default!;
    }
}