using FluentValidation;

using P3D.Legacy.Server.Abstractions.Extensions;

namespace P3D.Legacy.Server.Client.P3D.Options
{
    public sealed class P3DServerOptionsValidator : AbstractValidator<P3DServerOptions>
    {
        public P3DServerOptionsValidator()
        {
            RuleFor(x => x.IP).NotEmpty().IsIPAddress();
            RuleFor(x => x.Port).NotEmpty();
        }
    }

    public sealed record P3DServerOptions
    {
        public string IP { get; init; } = default!;
        public ushort Port { get; init; } = default!;
    }
}