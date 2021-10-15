using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Permissions
{
    public interface IPermissionQueries
    {
        Task<PermissionViewModel> GetByGameJoltAsync(ulong id, CancellationToken ct);
    }
}