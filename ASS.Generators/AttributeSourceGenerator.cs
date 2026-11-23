using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ASS.Generators;

[Generator]
public class AttributeSourceGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "ASS.Features.Attributes";
    private const string FluentAttributeName = "FluentAttribute";
    private const string FullyQualifiedAttributeNamespace = AttributeNamespace + "." + FluentAttributeName;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TemplateModel> models = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => BuildModel(ctx))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!.Value);

        context.RegisterSourceOutput(models, static (ctx, model) => Emit(ctx, model));
    }

    private static TemplateModel? BuildModel(GeneratorSyntaxContext ctx)
    {
        ClassDeclarationSyntax decl = (ClassDeclarationSyntax)ctx.Node;
        if (ctx.SemanticModel.GetDeclaredSymbol(decl) is not INamedTypeSymbol symbol)
            return null;

        if (!Helpers.HasAttributeInHierarchy(symbol, AttributeNamespace, FluentAttributeName))
            return null;

        ImmutableArray<PropertyModel>.Builder props = ImmutableArray.CreateBuilder<PropertyModel>();
        foreach (ISymbol member in symbol.GetMembers())
        {
            if (member is not IPropertySymbol prop)
                continue;
            if (prop.GetMethod is null || prop.SetMethod is null)
                continue;
            if (!Helpers.HasAttribute(prop, AttributeNamespace, FluentAttributeName, out AttributeData data))
                continue;

            string namePrefix = string.Empty;
            string propPostfix = string.Empty;
            if (Helpers.IsBool(prop))
            {
                bool? val;
                if (data.ConstructorArguments.IsEmpty)
                {
                    val = true;
                }
                else
                {
                    val = data.ConstructorArguments[0].Value as bool?;
                }

                if (val.HasValue)
                {
                    if (val.Value)
                        propPostfix = " = true";
                    else
                    {
                        namePrefix = "out";
                        propPostfix = " = false";
                    }
                }
            }

            string nullable = prop.Type.NullableAnnotation is NullableAnnotation.Annotated ? "?" : string.Empty;

            props.Add(new PropertyModel(
                namePrefix + prop.Name,
                prop.Name,
                prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + nullable,
                Helpers.Camel(prop.Name) + propPostfix));
        }

        return new TemplateModel(
            symbol.ContainingNamespace.ToDisplayString(),
            symbol.Name,
            props.ToImmutable());
    }

    private static void Emit(SourceProductionContext ctx, TemplateModel model)
    {
        StringBuilder sb = new();
        sb.AppendLine("// Source generated file");
        sb.AppendLine("#nullable enable");
        sb.AppendLine($"namespace {model.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"partial class {model.ClassName}");
        sb.AppendLine("{");

        foreach (PropertyModel p in model.FluentProperties)
        {
            sb.AppendLine($@"
    public {model.ClassName} With{p.MethodName}({p.TypeFqn} {p.ParamName})
    {{
        {p.PropertyName} = {p.ParamName};
        return this;
    }}");
        }

        sb.AppendLine("}");

        ctx.AddSource($"{model.ClassName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private record struct TemplateModel(string Namespace, string ClassName, ImmutableArray<PropertyModel> FluentProperties);

    private record struct PropertyModel(string MethodName, string PropertyName, string TypeFqn, string ParamName);
}