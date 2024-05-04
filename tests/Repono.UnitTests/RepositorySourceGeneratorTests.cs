namespace Repono.UnitTests;

public class RepositorySourceGeneratorTests
{
    [Fact]
    public void Test()
    {
        var source = @"
namespace Tests;

using Repono;

public interface IFoo {}

public class FooQuery : IQuery, IFoo
{    
}

internal sealed class FooQueryHandler : IQueryHandler<FooQuery>
{
    public Task ExecuteAsync(FooQuery query, CancellationToken cancellationToken)
    {
        return Task.Yield();
    }
}

public record BarQuery(string Value) : IQuery<decimal>;

internal sealed class BarQueryHandler : IQueryHandler<BarQuery, decimal>
{
    public async Task<decimal> ExecuteAsync(BarQuery query, CancellationToken cancellationToken)
    {
        await Task.Delay(1);
        return 100500;
    }
}

public record Baz() : IQuery;
";
        TestHelper.Verify(source);
    }
}
