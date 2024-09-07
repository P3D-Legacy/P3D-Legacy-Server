using P3D.Legacy.Common;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Domain.Services;

public interface IPlayerOriginGenerator
{
    Task<Origin> GenerateAsync(CancellationToken ct);
}