namespace Repono;

public interface IQueryHandler<TQuery>
    where TQuery : notnull, IQuery
{
    Task ExecuteAsync(TQuery query, CancellationToken cancellationToken);
}

public interface IQueryHandler<TQuery, TResult>
    where TQuery : notnull, IQuery<TResult>
{
    Task<TResult> ExecuteAsync(TQuery query, CancellationToken cancellationToken);
}
