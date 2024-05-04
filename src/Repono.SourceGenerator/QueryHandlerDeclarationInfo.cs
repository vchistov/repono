namespace Repono.SourceGenerator
{
    using Microsoft.CodeAnalysis;

    internal sealed class QueryHandlerDeclarationInfo
    {
        public QueryHandlerDeclarationInfo(INamedTypeSymbol handlerInterface, INamedTypeSymbol handler)
        {
            if (handlerInterface.TypeArguments.Length > 1)
            {
                QueryResultFullName = handlerInterface.TypeArguments[1].ToDisplayString();
            }

            QueryFullName = handlerInterface.TypeArguments[0].ToDisplayString();
            HandlerInterfaceFullName = handlerInterface.ToDisplayString();
            HandlerFullName = handler.ToDisplayString();
        }

        public bool IsResultableQuery
        {
            get
            {
                return !string.IsNullOrEmpty(QueryResultFullName);
            }
        }

        public string QueryFullName { get; private set; }

        public string? QueryResultFullName { get; private set; }

        public string HandlerInterfaceFullName { get; private set; }

        public string HandlerFullName { get; private set; }
    }
}
