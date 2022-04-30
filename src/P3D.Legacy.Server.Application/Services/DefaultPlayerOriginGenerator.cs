using P3D.Legacy.Common;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    [SuppressMessage("Performance", "CA1812")]
    internal class DefaultPlayerOriginGenerator : IPlayerOriginGenerator
    {
        private long _globalPlayerIncrement;

        public Task<Origin> GenerateAsync(CancellationToken ct) => Task.FromResult(Origin.FromNumber(Interlocked.Increment(ref _globalPlayerIncrement)));
    }
}