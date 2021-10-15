using Microsoft.AspNetCore.Http.Features;

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

        Origin Id { get; }
        string Name { get; }
        GameJoltId GameJoltId { get; }
        PermissionFlags Permissions { get; }
        IPAddress IPAddress { get; }

        IFeatureCollection Features { get; }

        Task AssignIdAsync(long id, CancellationToken ct);
        Task AssignPermissionsAsync(PermissionFlags permissions, CancellationToken ct);
    }
}