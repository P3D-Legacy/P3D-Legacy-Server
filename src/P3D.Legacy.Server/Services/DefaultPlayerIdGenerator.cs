using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public class DefaultPlayerIdGenerator : IPlayerIdGenerator
    {
        private ulong _globalPlayerIncrement;

        public Task<ulong> GenerateAsync(CancellationToken ct) => Task.FromResult(Interlocked.Increment(ref _globalPlayerIncrement));
    }
}