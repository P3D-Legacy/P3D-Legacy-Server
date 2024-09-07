using P3D.Legacy.Common;
using P3D.Legacy.Server.Domain.Services;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services;

internal class DefaultPlayerOriginGenerator : IPlayerOriginGenerator
{
    private long _globalPlayerIncrement;

    public Task<Origin> GenerateAsync(CancellationToken ct) => Task.FromResult(Origin.From(Interlocked.Increment(ref _globalPlayerIncrement)));
}