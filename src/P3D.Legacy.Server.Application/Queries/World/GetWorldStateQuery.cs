﻿using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.CQERS.Queries;

namespace P3D.Legacy.Server.Application.Queries.World
{
    public sealed record GetWorldStateQuery : IQuery<WorldState>;
}