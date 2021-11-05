using P3D.Legacy.Common;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Permissions
{
    public interface IPermissionQueries
    {
        Task<PermissionViewModel> GetByIdAsync(PlayerId id, CancellationToken ct);
    }
}