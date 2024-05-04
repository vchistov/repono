namespace Repono.IntegrationTests.TestData;

internal sealed class ExceptionTracingQueryDecorator<TQuery, TResult>(IInvocationTracker invocationTracker) : IQueryDecorator<TQuery, TResult>
    where TQuery : notnull
{
    public async Task<TResult> ExecuteAsync(TQuery query, QueryHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        try
        {
            invocationTracker.Track(GetType(), "begin");

            var result = await next();

            invocationTracker.Track(GetType(), "end");

            return result;
        }
        catch (Exception ex)
        {
            invocationTracker.Track(GetType(), "exception", ex.GetType().Name);
            throw;
        }
    }
}
