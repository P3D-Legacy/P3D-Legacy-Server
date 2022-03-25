using FluentValidation;

using P3D.Legacy.Server.Abstractions.Extensions;

namespace P3D.Legacy.Server.DiscordBot.Options
{
    public sealed class DiscordOptionsValidator : AbstractValidator<DiscordOptions>
    {
        public DiscordOptionsValidator()
        {
            //RuleFor(options => options.BotToken).NotEmpty().NotInteger().NotBoolean();
            RuleFor(options => options.PasstroughChannelId).NotEmpty().When(x => !string.IsNullOrEmpty(x.BotToken));
        }
    }

    public sealed record DiscordOptions
    {
        public string BotToken { get; init; } = default!;
        public ulong PasstroughChannelId { get; init; } = default!;
    }
}