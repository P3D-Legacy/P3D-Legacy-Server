using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public interface IPlayerIdGenerator
    {
        Task<ulong> GenerateAsync(CancellationToken ct);
    }
}