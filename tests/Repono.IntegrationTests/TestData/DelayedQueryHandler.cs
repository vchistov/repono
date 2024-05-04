namespace Repono.IntegrationTests.TestData;

using Repono.IntegrationTests.Artifacts;

internal sealed class DelayedQueryHandler(IInvocationTracker invocationTracker) : IQueryHandler<DelayedQuery>
{
    public async Task ExecuteAsync(DelayedQuery query, CancellationToken cancellationToken)
    {
        invocationTracker.Track(GetType(), "begin");
        await Task.Delay(query.Delay, cancellationToken);
        invocationTracker.Track(GetType(), "end");
    }
}
