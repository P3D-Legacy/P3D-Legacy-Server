using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Services.Permissions
{
    public interface IPermissionManager
    {
        Task<PermissionEntity> GetByIdAsync(PlayerId id, CancellationToken ct);
        Task<bool> SetPermissionsAsync(PlayerId id,PermissionFlags permissions, CancellationToken ct);
    }
}