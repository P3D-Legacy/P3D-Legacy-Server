using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public class DefaultPlayerIdGenerator : IPlayerIdGenerator
    {
        private long _globalPlayerIncrement;

        public Task<long> GenerateAsync(CancellationToken ct) => Task.FromResult(Interlocked.Increment(ref _globalPlayerIncrement));
    }
}