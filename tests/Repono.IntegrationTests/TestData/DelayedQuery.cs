namespace Repono.IntegrationTests.Artifacts;

internal sealed record DelayedQuery(TimeSpan Delay) : IQuery;
