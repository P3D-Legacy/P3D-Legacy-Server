using MediatR;

using P3D.Legacy.Server.Abstractions.Queries;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions.Services
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IMediator _mediator;

        public QueryDispatcher(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken ct)
        {
            return _mediator.Send(query, ct);
        }
    }
}