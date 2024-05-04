namespace Repono.IntegrationTests;

using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Moq;

using Repono.IntegrationTests.Artifacts;
using Repono.IntegrationTests.TestData;

public class RepositoryTests
{
    private readonly Mock<IInvocationTracker> _invocationTrackerMock = new Mock<IInvocationTracker>();
    private readonly IRepository _sut;

    public RepositoryTests()
    {
        var services = new ServiceCollection();
        services.AddRepono();
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IQueryDecorator<,>), typeof(ExceptionTracingQueryDecorator<,>), ServiceLifetime.Scoped));
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IQueryDecorator<,>), typeof(TracingQueryDecorator<,>), ServiceLifetime.Scoped));
        services.AddScoped(_ => _invocationTrackerMock.Object);

        _sut = services.BuildServiceProvider().GetRequiredService<IRepository>();
    }

    [Fact]
    public async Task ExecuteAsync_ResultlessQuery_TrackInvocation()
    {
        // Arrange
        const int DelayMs = 150;
        var sw = Stopwatch.StartNew();

        // Act
        await _sut.ExecuteAsync(new DelayedQuery(TimeSpan.FromMilliseconds(DelayMs)), default);

        // Assert
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds > DelayMs);
        _invocationTrackerMock.Verify(m => m.Track(typeof(ExceptionTracingQueryDecorator<DelayedQuery, object>), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(TracingQueryDecorator<DelayedQuery, object>), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(DelayedQueryHandler), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(DelayedQueryHandler), "end"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(TracingQueryDecorator<DelayedQuery, object>), "end"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(ExceptionTracingQueryDecorator<DelayedQuery, object>), "end"));
        _invocationTrackerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_ResultfullQuery_TrackInvocation()
    {
        // Arrange, Act
        var result = await _sut.ExecuteAsync(new GreetingQuery("Ivan"), default);

        // Assert
        Assert.Equal("Hello Ivan!", result);
        _invocationTrackerMock.Verify(m => m.Track(typeof(ExceptionTracingQueryDecorator<GreetingQuery, string>), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(TracingQueryDecorator<GreetingQuery, string>), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(GreetingQueryHandler), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(GreetingQueryHandler), "end"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(TracingQueryDecorator<GreetingQuery, string>), "end"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(ExceptionTracingQueryDecorator<GreetingQuery, string>), "end"));
        _invocationTrackerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_QueryWithException_TrackInvocation()
    {
        // Arrange, Act
        await Assert.ThrowsAsync<NotImplementedException>(() => _sut.ExecuteAsync(new ExceptionQuery(), default));

        // Assert
        _invocationTrackerMock.Verify(m => m.Track(typeof(ExceptionTracingQueryDecorator<ExceptionQuery, object>), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(TracingQueryDecorator<ExceptionQuery, object>), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(ExceptionQueryHandler), "begin"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(ExceptionQueryHandler), "end"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(TracingQueryDecorator<ExceptionQuery, object>), "end"));
        _invocationTrackerMock.Verify(m => m.Track(typeof(ExceptionTracingQueryDecorator<ExceptionQuery, object>), "exception", nameof(NotImplementedException)));
        _invocationTrackerMock.VerifyNoOtherCalls();
    }
}
