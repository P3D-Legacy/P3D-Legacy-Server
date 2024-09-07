namespace P3D.Legacy.Server.Domain.Queries;

public interface IBaseQuery<out TQueryResult> { }
public interface IQuery<out TQueryResult> : IBaseQuery<TQueryResult> { }