using P3D.Legacy.Server.Abstractions;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services;

public interface IPlayerContainerWriterAsync
{
    Task AddAsync(IPlayer player, CancellationToken ct);
    Task<bool> RemoveAsync(IPlayer player, CancellationToken ct);
}