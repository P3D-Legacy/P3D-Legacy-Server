using MediatR;

namespace P3D.Legacy.Server.Abstractions.Queries
{
    public interface IQuery<out TQueryResult> : IRequest<TQueryResult> { }
}