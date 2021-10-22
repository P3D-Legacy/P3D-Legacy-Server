using P3D.Legacy.Common;
using P3D.Legacy.Common.Monsters;

using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Monsters
{
    public interface IMonsterRepository
    {
        Task<IMonsterInstance> GetByDataAsync(DataItemStorage dataItems);
    }
}