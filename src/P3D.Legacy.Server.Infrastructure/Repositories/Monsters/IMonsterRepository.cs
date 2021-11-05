using P3D.Legacy.Common.Monsters;

using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Monsters
{
    public interface IMonsterRepository
    {
        Task<IMonsterInstance> GetByDataAsync(string monsterData);
    }
}