namespace Repono;

public interface IQueryDecorator<TQuery, TResult>
    where TQuery : notnull
{
    Task<TResult> ExecuteAsync(TQuery query, QueryHandlerDelegate<TResult> next, CancellationToken cancellationToken);
}
