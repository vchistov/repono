namespace Repono.IntegrationTests.TestData;

internal sealed class GreetingQueryHandler(IInvocationTracker invocationTracker) : IQueryHandler<GreetingQuery, string>
{
    public Task<string> ExecuteAsync(GreetingQuery query, CancellationToken cancellationToken)
    {
        invocationTracker.Track(GetType(), "begin");

        try
        {
            return Task.FromResult($"Hello {query.Name}!");
        }
        finally
        {
            invocationTracker.Track(GetType(), "end");
        }
    }
}
