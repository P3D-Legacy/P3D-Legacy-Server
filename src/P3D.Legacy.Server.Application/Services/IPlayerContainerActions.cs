using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public interface IPlayerContainerActions
    {
        Task<bool> KickAsync(IPlayer player, CancellationToken ct);
    }
}