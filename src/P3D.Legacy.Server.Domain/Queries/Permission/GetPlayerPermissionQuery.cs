using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Domain.Queries.Permission;

public sealed record GetPlayerPermissionQuery(PlayerId Id) : IQuery<PermissionViewModel?>;