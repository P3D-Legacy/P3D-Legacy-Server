using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Queries.Permissions
{
    public interface IPermissionQueries
    {
        Task<PermissionViewModel?> GetGameJoltAsync(ulong id, CancellationToken ct);
    }
}