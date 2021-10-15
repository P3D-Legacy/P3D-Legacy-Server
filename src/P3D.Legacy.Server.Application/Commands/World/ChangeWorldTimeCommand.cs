using MediatR;

using System;

namespace P3D.Legacy.Server.Application.Commands.World
{
    public record ChangeWorldTimeCommand(TimeSpan Time) : IRequest<CommandResult>;
}