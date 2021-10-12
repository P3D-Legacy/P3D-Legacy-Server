using P3D.Legacy.Server.Models;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public interface IPlayerContainerWriter
    {
        Task AddAsync(IPlayer player, CancellationToken ct);
        Task RemoveAsync(IPlayer player, CancellationToken ct);
    }
}