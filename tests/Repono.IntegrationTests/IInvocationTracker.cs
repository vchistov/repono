namespace Repono.IntegrationTests;

internal interface IInvocationTracker
{
    void Track(Type type, params string[] labels);
}
