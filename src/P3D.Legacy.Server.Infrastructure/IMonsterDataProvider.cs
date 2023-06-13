using P3D.Legacy.Common.Monsters;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure
{
    public interface IMonsterDataProvider
    {
        Task<(IMonsterStaticData, IItemInstance?)> GetStaticDataAsync(int id, int itemId, CancellationToken ct);
    }
}