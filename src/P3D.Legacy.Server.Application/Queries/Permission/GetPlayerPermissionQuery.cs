﻿using P3D.Legacy.Common;
using P3D.Legacy.Server.CQERS.Queries;

namespace P3D.Legacy.Server.Application.Queries.Permission
{
    public sealed record GetPlayerPermissionQuery(PlayerId Id) : IQuery<PermissionViewModel?>;
}