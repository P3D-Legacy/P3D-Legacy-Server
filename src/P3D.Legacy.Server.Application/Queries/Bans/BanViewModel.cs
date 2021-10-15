using P3D.Legacy.Common;

using System;
using System.Net;

namespace P3D.Legacy.Server.Application.Queries.Bans
{
    public record BanViewModel(GameJoltId Id, string Name, IPAddress IP, string Reason, DateTimeOffset? Expiration);
}