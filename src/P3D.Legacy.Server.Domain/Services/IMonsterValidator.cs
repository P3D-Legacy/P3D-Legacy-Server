using P3D.Legacy.Common.Monsters;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Domain.Services;

public interface IMonsterValidator
{
    Task<bool> ValidateAsync(IMonsterInstance monsterInstance, CancellationToken ct);
}