using MediatR;

using P3D.Legacy.Common;

using System;
using System.Net;

namespace P3D.Legacy.Server.Application.Commands.Administration
{
    public record BanPlayerCommand(GameJoltId Id, string Name, IPAddress IP, string Reason, DateTimeOffset? Expiration) : IRequest<CommandResult>;
}