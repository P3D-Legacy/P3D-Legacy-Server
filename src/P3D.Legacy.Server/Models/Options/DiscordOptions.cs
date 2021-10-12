namespace P3D.Legacy.Server.Models.Options
{
    /*
    public sealed class DiscordOptionsValidator : AbstractValidator<DiscordOptions>
    {
        public DiscordOptionsValidator()
        {
            RuleFor(options => options.BotToken).NotEmpty().NotInteger().NotBoolean();
        }
    }
    */

    public sealed record DiscordOptions
    {
        public ulong PasstroughChannelId { get; init; } = default!;
        public string BotToken { get; init; } = default!;
    }
}