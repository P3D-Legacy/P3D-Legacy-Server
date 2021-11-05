using P3D.Legacy.Common;

using System.Collections.Generic;

namespace P3D.Legacy.Server.Infrastructure.Models.Mutes
{
    public record MuteEntity(PlayerId Id, IReadOnlyList<PlayerId> MutedPlayerIds);
}