﻿namespace Repono.SourceGenerator
{
    using System.Collections.Immutable;
    using System.Text;

    internal sealed class RepositorySourceBuilder
    {
        private const string DefaultLifetime = "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped";
        private const string DefaultRepositoryName = "DefaultRepository";

        private readonly StringBuilder _builder = new StringBuilder();

        private RepositorySourceBuilder()
        {
        }

        public static string Build(ImmutableArray<QueryHandlerDeclarationInfo> declarations)
        {
            return new RepositorySourceBuilder().BuildInternal(declarations);
        }

        private string BuildInternal(ImmutableArray<QueryHandlerDeclarationInfo> declarations)
        {
            AppendHeader();
            AppendServiceCollectionExtensions(declarations);
            AppendRepositoryDeclaration(declarations);

            return _builder.ToString();
        }

        private void AppendHeader()
        {
            _builder.AppendLine("// <auto-generated/>");
            _builder.AppendLine("#nullable enable");
            _builder.AppendLine("namespace Repono;");
        }

        private void AppendServiceCollectionExtensions(ImmutableArray<QueryHandlerDeclarationInfo> declarations)
        {
            _builder.AppendLine("public static class ServiceCollectionExtensions");
            _builder.AppendLine("{");
            _builder.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddRepono(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
            _builder.AppendLine("    {");

            foreach (var info in declarations)
            {
                AppendHandlerRegistration(info);
            }

            AppendRepositoryRegistration();

            _builder.AppendLine("        return services;");
            _builder.AppendLine("    }");
            _builder.AppendLine("}");
            _builder.AppendLine();
        }

        private void AppendHandlerRegistration(QueryHandlerDeclarationInfo info)
        {
            _builder.Append("        services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(global::");
            // That's ok for now to skip adding 'global::' prefix for Query type and result type arguments
            _builder.Append(info.HandlerInterfaceFullName);
            _builder.Append("), typeof(global::");
            _builder.Append(info.HandlerFullName);
            _builder.Append("), ");
            _builder.Append(DefaultLifetime);
            _builder.AppendLine("));");
        }

        private void AppendRepositoryRegistration()
        {
            _builder.Append("        services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(global::Repono.IRepository), typeof(");
            _builder.Append(DefaultRepositoryName);
            _builder.Append("), ");
            _builder.Append(DefaultLifetime);
            _builder.AppendLine("));");
        }

        private void AppendRepositoryDeclaration(ImmutableArray<QueryHandlerDeclarationInfo> declarations)
        {
            _builder.Append("internal sealed class ").Append(DefaultRepositoryName).AppendLine(" : global::Repono.IRepository");
            _builder.AppendLine("{");
            _builder.AppendLine("    private readonly global::System.IServiceProvider _serviceProvider;");
            _builder.Append("    public ").Append(DefaultRepositoryName).AppendLine("(global::System.IServiceProvider serviceProvider)");
            _builder.AppendLine("    {");
            _builder.AppendLine("        _serviceProvider = serviceProvider;");
            _builder.AppendLine("    }");

            AppendResultlessExecuteMethodDeclaration(declarations);
            AppendResultfullExecuteMethodDeclaration(declarations);

            AppendRepositoryUtilityMethods();

            _builder.AppendLine("}");
        }

        private void AppendResultlessExecuteMethodDeclaration(ImmutableArray<QueryHandlerDeclarationInfo> declarations)
        {
            _builder.AppendLine("    public global::System.Threading.Tasks.Task ExecuteAsync(global::Repono.IQuery query, global::System.Threading.CancellationToken cancellationToken)");
            _builder.AppendLine("    {");

            int index = 1;
            foreach (var info in declarations)
            {
                if (info.IsResultableQuery)
                {
                    continue;
                }

                _builder.Append("        if (query is global::").Append(info.QueryFullName).Append(" q").Append(index).AppendLine(")");
                _builder.AppendLine("        {");
                _builder.Append("            return ExecuteInternalAsync(q").Append(index).AppendLine(", cancellationToken);");
                _builder.AppendLine("        }");
                index++;
            }

            _builder.AppendLine("        throw new global::System.InvalidOperationException($\"Query handler not found for {query.GetType()}.\");");
            _builder.AppendLine("    }");
        }

        private void AppendResultfullExecuteMethodDeclaration(ImmutableArray<QueryHandlerDeclarationInfo> declarations)
        {
            _builder.AppendLine("    public global::System.Threading.Tasks.Task<TResult> ExecuteAsync<TResult>(global::Repono.IQuery<TResult> query, global::System.Threading.CancellationToken cancellationToken)");
            _builder.AppendLine("    {");

            int index = 1;
            foreach (var info in declarations)
            {
                if (!info.IsResultableQuery)
                {
                    continue;
                }

                _builder.Append("        if (query is global::").Append(info.QueryFullName).Append(" q").Append(index).AppendLine(")");
                _builder.AppendLine("        {");
                _builder.Append("            var task = ExecuteInternalAsync<global::").Append(info.QueryFullName).Append(",").Append(info.QueryResultFullName).Append(">(q").Append(index).AppendLine(", cancellationToken);");
                _builder.AppendLine("            return (task as global::System.Threading.Tasks.Task<TResult>)!;");
                _builder.AppendLine("        }");
                index++;
            }

            _builder.AppendLine("        throw new global::System.InvalidOperationException($\"Query handler not found for {query.GetType()}.\");");
            _builder.AppendLine("    }");
        }

        private void AppendRepositoryUtilityMethods()
        {
            _builder.Append(@"
    private global::System.Threading.Tasks.Task ExecuteInternalAsync<TQuery>(TQuery query, global::System.Threading.CancellationToken cancellationToken)
        where TQuery : notnull, global::Repono.IQuery
    {
        var decorators = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetServices<global::Repono.IQueryDecorator<TQuery, global::System.Object>>(_serviceProvider) ?? global::System.Linq.Enumerable.Empty<global::Repono.IQueryDecorator<TQuery, global::System.Object>>();
        var handler = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<global::Repono.IQueryHandler<TQuery>>(_serviceProvider);

        var adapter = new QueryResultlessAdapter<TQuery>(handler);
        var pipeline = BuildHandlerPipeline(decorators, adapter.ExecuteAsync);

        return pipeline.Invoke(query, cancellationToken);
    }

    private global::System.Threading.Tasks.Task<TResult> ExecuteInternalAsync<TQuery, TResult>(TQuery query, global::System.Threading.CancellationToken cancellationToken)
        where TQuery : notnull, global::Repono.IQuery<TResult>
    {
        var decorators = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetServices<global::Repono.IQueryDecorator<TQuery, TResult>>(_serviceProvider) ?? global::System.Linq.Enumerable.Empty<global::Repono.IQueryDecorator<TQuery, TResult>>();
        var handler = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<global::Repono.IQueryHandler<TQuery, TResult>>(_serviceProvider);

        var pipeline = BuildHandlerPipeline(decorators, handler.ExecuteAsync);

        return pipeline.Invoke(query, cancellationToken);
    }

    private static global::System.Func<TQuery, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task<TResult>> BuildHandlerPipeline<TQuery, TResult>(
        global::System.Collections.Generic.IEnumerable<global::Repono.IQueryDecorator<TQuery, TResult>> decorators,
        global::System.Func<TQuery, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task<TResult>> handlerExecuteFunc)
        where TQuery : notnull
    {
        return global::System.Linq.Enumerable.Aggregate(
            decorators,
            handlerExecuteFunc,
            (h, d) => (q, ct) => d.ExecuteAsync(q, () => h(q, ct), ct));
    }

    private struct QueryResultlessAdapter<TQuery>
        where TQuery : notnull, global::Repono.IQuery
    {
        private readonly static global::System.Object VoidResult = new global::System.Object();
        private readonly global::Repono.IQueryHandler<TQuery> _innerHandler;

        public QueryResultlessAdapter(global::Repono.IQueryHandler<TQuery> innerHandler)
        {
            _innerHandler = innerHandler;
        }

        public async global::System.Threading.Tasks.Task<global::System.Object> ExecuteAsync(TQuery query, global::System.Threading.CancellationToken cancellationToken)
        {
            await _innerHandler.ExecuteAsync(query, cancellationToken);
            return VoidResult;
        }
    }
");

            ////_builder.AppendLine("    private global::System.Threading.Tasks.Task ExecuteInternalAsync<TQuery>(TQuery query, global::System.Threading.CancellationToken cancellationToken) where TQuery : notnull, global::Repono.IQuery");
            ////_builder.AppendLine("    {");
            ////_builder.AppendLine("        var decorators = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetServices<global::Repono.IQueryDecorator<TQuery, global::System.Object>>(_serviceProvider) ?? global::System.Linq.Enumerable.Empty<global::Repono.IQueryDecorator<TQuery, global::System.Object>>();");
            ////_builder.AppendLine("        var handler = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<global::Repono.IQueryHandler<TQuery>>(_serviceProvider);");
            ////_builder.AppendLine("        var adapter = new QueryResultlessAdapter<TQuery>(handler);");
            ////_builder.AppendLine("        var pipeline = BuildHandlerPipeline(decorators, adapter.ExecuteAsync);");
            ////_builder.AppendLine("        return pipeline.Invoke(query, cancellationToken);");
            ////_builder.AppendLine("    }");

            ////_builder.AppendLine("    private global::System.Threading.Tasks.Task<TResult> ExecuteInternalAsync<TQuery, TResult>(TQuery query, global::System.Threading.CancellationToken cancellationToken) where TQuery : notnull, global::Repono.IQuery<TResult>");
            ////_builder.AppendLine("    {");
            ////_builder.AppendLine("        var decorators = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetServices<global::Repono.IQueryDecorator<TQuery, TResult>>(_serviceProvider) ?? global::System.Linq.Enumerable.Empty<global::Repono.IQueryDecorator<TQuery, TResult>>();");
            ////_builder.AppendLine("        var handler = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<global::Repono.IQueryHandler<TQuery, TResult>>(_serviceProvider);");
            ////_builder.AppendLine("        var pipeline = BuildHandlerPipeline(decorators, handler.ExecuteAsync);");
            ////_builder.AppendLine("        return pipeline.Invoke(query, cancellationToken);");
            ////_builder.AppendLine("    }");

            ////_builder.AppendLine("    private static global::System.Func<TQuery, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task<TResult>> BuildHandlerPipeline<TQuery, TResult>(global::System.Collections.Generic.IEnumerable<global::Repono.IQueryDecorator<TQuery, TResult>> decorators, global::System.Func<TQuery, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task<TResult>> handlerExecuteFunc) where TQuery : notnull");
            ////_builder.AppendLine("    {");
            ////_builder.AppendLine("        return global::System.Linq.Enumerable.Aggregate(decorators, handlerExecuteFunc, (h, d) => (q, ct) => d.ExecuteAsync(q, () => h(q, ct), ct));");
            ////_builder.AppendLine("    }");

            ////_builder.AppendLine("    private struct QueryResultlessAdapter<TQuery> where TQuery : notnull, global::Repono.IQuery");
            ////_builder.AppendLine("    {");
            ////_builder.AppendLine("        private readonly static global::System.Object VoidResult = new global::System.Object();");
            ////_builder.AppendLine("        private readonly global::Repono.IQueryHandler<TQuery> _innerHandler;");
            ////_builder.AppendLine("        public QueryResultlessAdapter(global::Repono.IQueryHandler<TQuery> innerHandler)");
            ////_builder.AppendLine("        {");
            ////_builder.AppendLine("            _innerHandler = innerHandler;");
            ////_builder.AppendLine("        }");
            ////_builder.AppendLine("        public async global::System.Threading.Tasks.Task<global::System.Object> ExecuteAsync(TQuery query, global::System.Threading.CancellationToken cancellationToken)");
            ////_builder.AppendLine("        {");
            ////_builder.AppendLine("            await _innerHandler.ExecuteAsync(query, cancellationToken);");
            ////_builder.AppendLine("            return VoidResult;");
            ////_builder.AppendLine("        }");
            ////_builder.AppendLine("    }");
        }
    }
}
