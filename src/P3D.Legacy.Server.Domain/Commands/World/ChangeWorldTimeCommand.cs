using System;

namespace P3D.Legacy.Server.Domain.Commands.World;

public sealed record ChangeWorldTimeCommand(TimeSpan Time) : ICommand;