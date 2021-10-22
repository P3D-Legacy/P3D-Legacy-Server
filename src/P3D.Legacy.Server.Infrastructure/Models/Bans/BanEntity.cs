using P3D.Legacy.Common;

using System;
using System.Net;

namespace P3D.Legacy.Server.Infrastructure.Models.Bans
{
    public record BanEntity(GameJoltId Id, string Name, IPAddress Ip, string Reason, DateTimeOffset? Expiration);
}