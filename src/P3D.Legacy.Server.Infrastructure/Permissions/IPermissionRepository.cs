using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Permissions
{
    public interface IPermissionRepository
    {
        Task<PermissionEntity> GetByGameJoltAsync(GameJoltId id, CancellationToken ct);
    }
}
