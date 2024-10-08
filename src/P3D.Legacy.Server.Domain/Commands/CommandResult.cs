﻿namespace P3D.Legacy.Server.Domain.Commands;

public record CommandResult(bool IsSuccess)
{
    public static CommandResult Success { get; } = new(true);
    public static CommandResult Failure { get; } = new(false);
}