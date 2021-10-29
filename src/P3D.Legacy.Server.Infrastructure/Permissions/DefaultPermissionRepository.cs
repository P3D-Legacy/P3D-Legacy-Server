using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Permissions
{
    public class DefaultPermissionRepository : IPermissionRepository
    {
        public Task<PermissionEntity> GetByGameJoltAsync(GameJoltId id, CancellationToken ct)
        {
            return Task.FromResult(new PermissionEntity(PermissionFlags.UnVerified));
        }
    }
}