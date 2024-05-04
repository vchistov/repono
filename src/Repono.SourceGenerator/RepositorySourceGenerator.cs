namespace Repono.SourceGenerator
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Generator]
    public sealed class RepositorySourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pipeline =
                context.SyntaxProvider.CreateSyntaxProvider(
                    (node, _) => IsSyntaxTargetForGeneration(node),
                    (syntax, ct) => GetSemanticTargetForGeneration(syntax, ct))
                    .Where(t => t != null)
                    .Collect();

            context.RegisterSourceOutput(pipeline, (ctx, nodes) => Build(ctx, nodes!));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node is BaseListSyntax bl && bl.Parent is ClassDeclarationSyntax;
        }

        private static QueryHandlerDeclarationInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var baseListSyntax = (BaseListSyntax)context.Node;
            foreach (var baseListItem in baseListSyntax.Types)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsQueryHandlerTypeSyntax(baseListItem.Type))
                {
                    continue;
                }

                var handlerInterfaceSemanticNode = GetHandlerInterfaceSemanticNode(context.SemanticModel, baseListItem.Type, cancellationToken);
                if (handlerInterfaceSemanticNode == null)
                {
                    continue;
                }

                var handlerImplementationSemanticNode = GetHandlerImplementationSemanticNode(context.SemanticModel, (ClassDeclarationSyntax)baseListSyntax.Parent!, cancellationToken);
                if (handlerImplementationSemanticNode == null)
                {
                    continue;
                }

                return new QueryHandlerDeclarationInfo(handlerInterfaceSemanticNode, handlerImplementationSemanticNode);
            }

            return null;
        }

        private static bool IsQueryHandlerTypeSyntax(TypeSyntax typeSyntax)
        {
            return typeSyntax is GenericNameSyntax genericType
                && StringComparer.Ordinal.Equals(genericType.Identifier.ValueText, "IQueryHandler");
        }

        private static INamedTypeSymbol? GetHandlerInterfaceSemanticNode(SemanticModel semanticModel, SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            var semanticNode = semanticModel.GetSymbolInfo(syntaxNode, cancellationToken).Symbol as INamedTypeSymbol;

            if (semanticNode != null
                && semanticNode.TypeKind == TypeKind.Interface
                && StringComparer.Ordinal.Equals(semanticNode.ContainingNamespace.Name, "Repono"))
            {
                return semanticNode;
            }

            return null;
        }

        private static INamedTypeSymbol? GetHandlerImplementationSemanticNode(SemanticModel semanticModel, ClassDeclarationSyntax syntaxNode, CancellationToken cancellationToken)
        {
            var semanticNode = semanticModel.GetDeclaredSymbol(syntaxNode, cancellationToken) as INamedTypeSymbol;

            if (semanticNode != null && semanticNode.TypeKind == TypeKind.Class && !semanticNode.IsAbstract)
            {
                return semanticNode;
            }

            return null;
        }

        private void Build(
            SourceProductionContext context,
            ImmutableArray<QueryHandlerDeclarationInfo> declarations)
        {
            context.AddSource("Repository.g.cs", RepositorySourceBuilder.Build(declarations));
        }
    }
}
