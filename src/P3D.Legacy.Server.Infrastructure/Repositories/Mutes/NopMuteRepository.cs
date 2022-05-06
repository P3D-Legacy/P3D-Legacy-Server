using P3D.Legacy.Common;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Mutes
{
    public class NopMuteRepository : IMuteRepository
    {
        public IAsyncEnumerable<PlayerId> GetAllAsync(PlayerId id, CancellationToken ct) => AsyncEnumerable.Empty<PlayerId>();
        public Task<bool> IsMutedAsync(PlayerId id, PlayerId toCheckId, CancellationToken ct) => Task.FromResult(false);
        public Task<bool> MuteAsync(PlayerId id, PlayerId toMuteId, CancellationToken ct) => Task.FromResult(false);
        public Task<bool> UnmuteAsync(PlayerId id, PlayerId toUnmuteId, CancellationToken ct) => Task.FromResult(false);
    }
}