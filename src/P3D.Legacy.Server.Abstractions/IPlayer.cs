using P3D.Legacy.Common;

using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions
{
    public interface IPlayer
    {
        static IPlayer Server { get; } = new ServerPlayer();

        string ConnectionId { get; }

        PlayerId Id { get; }
        Origin Origin { get; }
        string Name { get; }
        GameJoltId GameJoltId { get; }
        PermissionFlags Permissions { get; }
        IPEndPoint IPEndPoint { get; }

        Task AssignIdAsync(PlayerId id, CancellationToken ct);
        Task AssignOriginAsync(Origin origin, CancellationToken ct);
        Task AssignPermissionsAsync(PermissionFlags permissions, CancellationToken ct);

        Task KickAsync(string reason, CancellationToken ct);
    }
}