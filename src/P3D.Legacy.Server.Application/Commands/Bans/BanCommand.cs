using MediatR;

using P3D.Legacy.Common;

using System;
using System.Net;

namespace P3D.Legacy.Server.Application.Commands.Bans
{
    public record BanCommand(GameJoltId Id, string Name, IPAddress IP, string Reason, DateTimeOffset? Expiration) : IRequest<CommandResult>;
}
