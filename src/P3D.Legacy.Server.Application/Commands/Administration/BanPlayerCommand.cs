using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions.Commands;

using System;
using System.Net;

namespace P3D.Legacy.Server.Application.Commands.Administration
{
    public sealed record BanPlayerCommand(PlayerId BannerId, PlayerId Id, IPAddress IP, ulong ReasonId, string Reason, DateTimeOffset? Expiration) : ICommand;
}