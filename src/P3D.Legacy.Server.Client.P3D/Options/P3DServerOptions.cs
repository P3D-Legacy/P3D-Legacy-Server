﻿using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

namespace P3D.Legacy.Server.Client.P3D.Options;

public sealed class P3DServerOptionsValidator : AbstractValidator<P3DServerOptions>
{
    public P3DServerOptionsValidator()
    {
        RuleFor(static x => x.IP).NotEmpty().IsIPAddress();
        RuleFor(static x => x.Port).NotEmpty();
        RuleFor(static x => x.PortForwardTimeoutMilliseconds).GreaterThanOrEqualTo(0).When(static x => x.PortForward);
    }
}

public sealed record P3DServerOptions
{
    public required string IP { get; init; }
    public required ushort Port { get; init; }
    public required bool PortForward { get; init; } = true;
    public required int PortForwardTimeoutMilliseconds { get; init; } = 10000;
}