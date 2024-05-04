namespace Repono.IntegrationTests.TestData;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExceptionQueryHandler(IInvocationTracker invocationTracker) : IQueryHandler<ExceptionQuery>
{
    public Task ExecuteAsync(ExceptionQuery query, CancellationToken cancellationToken)
    {
        invocationTracker.Track(GetType(), "begin");

        try
        {
            throw new NotImplementedException();
        }
        finally
        {
            invocationTracker.Track(GetType(), "end");
        }
    }
}
