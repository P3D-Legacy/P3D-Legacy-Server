using P3D.Legacy.Common;

using System.Collections.Generic;

namespace P3D.Legacy.Server.Domain.Entities.Mutes;

public record MuteEntity(PlayerId Id, IReadOnlyList<PlayerId> MutedPlayerIds);