namespace Repono.IntegrationTests.TestData;

public record GreetingQuery(string Name) : IQuery<string>;
