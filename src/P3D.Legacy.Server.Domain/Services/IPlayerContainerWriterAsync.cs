using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Domain.Services;

public interface IPlayerContainerWriterAsync
{
    Task AddAsync(IPlayer player, CancellationToken ct);
    Task<bool> RemoveAsync(IPlayer player, CancellationToken ct);
}