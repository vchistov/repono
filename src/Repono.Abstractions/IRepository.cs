namespace Repono;

public interface IRepository
{
    Task ExecuteAsync(IQuery query, CancellationToken cancellationToken);

    Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
}
