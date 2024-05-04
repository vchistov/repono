namespace Repono.IntegrationTests.TestData;

internal sealed class TracingQueryDecorator<TQuery, TResult>(IInvocationTracker invocationTracker) : IQueryDecorator<TQuery, TResult>
    where TQuery : notnull
{
    public async Task<TResult> ExecuteAsync(TQuery query, QueryHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        invocationTracker.Track(GetType(), "begin");

        try
        {
            return await next();
        }
        finally
        {
            invocationTracker.Track(GetType(), "end");
        }
    }
}
