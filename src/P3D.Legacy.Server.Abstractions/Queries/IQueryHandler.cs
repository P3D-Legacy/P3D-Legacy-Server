using MediatR;

namespace P3D.Legacy.Server.Abstractions.Queries
{
    public interface IQueryHandler { }
    public interface IQueryHandler<in TQuery, TQueryResult> : IQueryHandler, IRequestHandler<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult> { }
}