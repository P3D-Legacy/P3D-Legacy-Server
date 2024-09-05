using P3D.Legacy.Common;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Mutes;

public interface IMuteRepository
{
    IAsyncEnumerable<PlayerId> GetAllAsync(PlayerId id, CancellationToken ct);
    Task<bool> IsMutedAsync(PlayerId id, PlayerId toCheckId, CancellationToken ct);
    Task<bool> MuteAsync(PlayerId id, PlayerId toMuteId, CancellationToken ct);
    Task<bool> UnmuteAsync(PlayerId id, PlayerId toUnmuteId, CancellationToken ct);
}