using P3D.Legacy.Common.Monsters;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Domain.Services;

// TODO: This seem to be P3D Specific?
public interface IMonsterDataProvider
{
    Task<(IMonsterStaticData, IItemInstance?)> GetStaticDataAsync(int id, int itemId, CancellationToken ct);
}