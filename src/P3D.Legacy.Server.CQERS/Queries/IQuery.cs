namespace P3D.Legacy.Server.CQERS.Queries
{
    public interface IBaseQuery<out TQueryResult> { }
    public interface IQuery<out TQueryResult> : IBaseQuery<TQueryResult> { }
}