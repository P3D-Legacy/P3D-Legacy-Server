using P3D.Legacy.Server.Abstractions;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public interface IPlayerContainerReader
    {
        Task<IPlayer?> GetAsync(long id, CancellationToken ct);
        IAsyncEnumerable<IPlayer> GetAllAsync(CancellationToken ct);
        IEnumerable<IPlayer> GetAll();
    }
}