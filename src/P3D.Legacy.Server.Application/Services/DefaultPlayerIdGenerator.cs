using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    internal class DefaultPlayerIdGenerator : IPlayerIdGenerator
    {
        private long _globalPlayerIncrement;

        public Task<long> GenerateAsync(CancellationToken ct) => Task.FromResult(Interlocked.Increment(ref _globalPlayerIncrement));
    }
}