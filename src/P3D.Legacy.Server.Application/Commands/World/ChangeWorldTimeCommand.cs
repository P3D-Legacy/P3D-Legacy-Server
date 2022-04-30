using P3D.Legacy.Server.Abstractions.Commands;

using System;

namespace P3D.Legacy.Server.Application.Commands.World
{
    public sealed record ChangeWorldTimeCommand(TimeSpan Time) : ICommand;
}