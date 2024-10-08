﻿using P3D.Legacy.Server.Domain.Queries;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query;

public interface IQueryBehavior<in TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
{
    int Order { get; }

    Task<TQueryResult> HandleAsync(TQuery query, QueryHandlerDelegate<TQueryResult> next, CancellationToken ct);
}