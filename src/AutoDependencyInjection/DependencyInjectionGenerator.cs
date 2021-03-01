using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDependencyInjection
{
    [Generator]
    internal class DependencyInjectionGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var compilation = context.Compilation;
            var classSymbols = new List<INamedTypeSymbol>();

            foreach (var cls in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(cls.SyntaxTree);

                var symbol = model.GetDeclaredSymbol(cls);

                if (symbol.GetMembers().OfType<IFieldSymbol>()
                     .Any(x => x.CanBeReferencedByName && x.IsReadOnly && !x.IsStatic && !HasInitializer(x)))
                {
                    classSymbols.Add(symbol); 
                }
            }

            var classConstructorsSource = new StringBuilder();

            foreach (var symbol in classSymbols)
            {
                classConstructorsSource.Append(CreateConstructor(symbol));
            }

            var source = new StringBuilder();

            context.AddSource("AutoDependencyInjection.g.cs", SourceText.From(classConstructorsSource.ToString(), Encoding.UTF8));
        }

        private string CreateConstructor(INamedTypeSymbol symbol)
        {
            string namespaceName = symbol.ContainingNamespace.ToDisplayString();

            var fields = symbol.GetMembers().OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsReadOnly && !x.IsStatic && !HasInitializer(x))
                .Select(it => new { Type = it.Type.ToDisplayString(), ParameterName = ToCamelCase(it.Name), it.Name })
                .ToList();

            var arguments = fields.Select(it => $"{it.Type} {it.ParameterName}");

            var elo = new StringBuilder();

            foreach (var field in fields)
            {
                elo.Append($@"this.{field.Name} = {field.ParameterName};");
            }

            var source = new StringBuilder($@"
namespace {namespaceName}
{{
    public partial class {symbol.Name}
    {{
        public {symbol.Name}({string.Join(", ", arguments)})
        {{
            {elo}
        }}
    }}
}}
");

            return source.ToString();
        }

        private static bool HasInitializer(IFieldSymbol symbol)
        {
            var field = symbol.DeclaringSyntaxReferences.ElementAtOrDefault(0)?.GetSyntax() as VariableDeclaratorSyntax;
            return field?.Initializer != null;
        }

        private static string ToCamelCase(string name)
        {
            name = name.TrimStart('_');
            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }
    }
}
