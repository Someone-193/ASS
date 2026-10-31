using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ASS.Generators;

[Generator]
public class TemplateSourceGenerator : IIncrementalGenerator
{
    private const string TemplateNamespace = "ASS.Features.Templates";
    private const string TemplateTypeName = "ASSTemplate";
    private const string FullyQualifiedBaseType = TemplateNamespace + "." + TemplateTypeName;

    private const string AttributeNamespace = "ASS.Features.Attributes";
    private const string AttributeName = "CanSyncAttribute";
    private const string FullyQualifiedAttributeNamespace = AttributeNamespace + "." + AttributeName;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TemplateModel> models = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
                transform: static (ctx, _) => BuildModel(ctx))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!.Value);

        context.RegisterSourceOutput(models, static (ctx, model) => Emit(ctx, model));
    }

    private record struct TemplateModel(string Namespace, string ClassName, string InstanceName, bool CanSync, ImmutableArray<PropertyModel> FluentProperties);

    private record struct PropertyModel(string MethodName, string PropertyName, string TypeFqn, string ParamName);

    private static TemplateModel? BuildModel(GeneratorSyntaxContext ctx)
    {
        ClassDeclarationSyntax decl = (ClassDeclarationSyntax)ctx.Node;
        if (ctx.SemanticModel.GetDeclaredSymbol(decl) is not INamedTypeSymbol symbol)
            return null;

        if (symbol.IsAbstract)
            return null;

        bool derivesFromBase = false;
        for (INamedTypeSymbol? b = symbol.BaseType; b is not null; b = b.BaseType)
        {
            if (b.ToDisplayString() != FullyQualifiedBaseType)
                continue;

            derivesFromBase = true;
            break;
        }

        if (!derivesFromBase)
            return null;

        bool canSync = HasAttributeInHierarchy(symbol, AttributeNamespace, AttributeName);

        ImmutableArray<PropertyModel>.Builder props = ImmutableArray.CreateBuilder<PropertyModel>();
        foreach (ISymbol member in symbol.GetMembers())
        {
            if (member is not IPropertySymbol prop)
                continue;
            if (prop.GetMethod is null || prop.SetMethod is null)
                continue;

            props.Add(new PropertyModel(
                IsInstanceHandler(prop) ? "Handler" : prop.Name,
                prop.Name,
                prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Camel(prop.Name) + (IsBool(prop) ? " = true" : string.Empty)));
        }

        int len = symbol.Name.IndexOf("Template", StringComparison.Ordinal);

        if (len is -1)
            return null;

        string instanceName = symbol.Name.Substring(0, len) + "Instance";

        return new TemplateModel(
            symbol.ContainingNamespace.ToDisplayString(),
            symbol.Name,
            instanceName,
            canSync,
            props.ToImmutable());
    }

    private static void Emit(SourceProductionContext ctx, TemplateModel model)
    {
        StringBuilder sb = new();
        sb.AppendLine("// Source generated file");
        sb.AppendLine("#nullable enable");
        sb.AppendLine($"namespace {model.Namespace};");
        sb.AppendLine();
        sb.AppendLine("""
                      using System.Collections.Generic;

                      using ASS.Features;
                      using ASS.Features.Instances;
                      using ASS.Features.Instances.Displays;

                      using LabApi.Features.Wrappers;

                      using UnityEngine.Pool;
                      """);
        sb.AppendLine();
        sb.AppendLine($"partial class {model.ClassName}");
        sb.AppendLine("{");

        sb.AppendLine($$"""
                            private {{model.ClassName}}(string? key, int? id)
                                : base(key, id)
                            {
                            }

                            public Dictionary<Player, {{model.InstanceName}}> Instances { get; } = new();
                        """);

        sb.AppendLine();

        if (model.CanSync)
        {
            sb.AppendLine("    public bool AutoSync { get; set; } = true;");
            sb.AppendLine();
        }

        sb.AppendLine($$"""
                            public static {{model.ClassName}} Create(string key) => new(key, null);

                            public static {{model.ClassName}} Create(int id) => new(null, id);

                            public {{model.ClassName}} InitializeOnJoin(bool initialize = true)
                            {
                                if (initialize)
                                {
                                    ASSNetworking.InitializingSettings.Add(this);
                                }
                                else
                                {
                                    ASSNetworking.InitializingSettings.Remove(this);
                                }
                            
                                return this;
                            }
                        
                            public {{model.ClassName}} WithLabel(string label)
                            {
                                Label = label;
                                return this;
                            }

                            public {{model.ClassName}} WithHint(string hint)
                            {
                                Hint = hint;
                                return this;
                            }

                            public {{model.ClassName}} WithCollectionId(byte collectionId)
                            {
                                CollectionId = collectionId;
                                return this;
                            }
                        """);

        if (model.CanSync)
            sb.AppendLine($$"""
                                public {{model.ClassName}} WithoutAutoSync(bool autoSync = false)
                                    {
                                        AutoSync = autoSync;
                                        return this;
                                    }
                            """);

        foreach (PropertyModel p in model.FluentProperties)
        {
            sb.AppendLine($@"
    public {model.ClassName} With{p.MethodName}({p.TypeFqn} {p.ParamName})
    {{
        {p.PropertyName} = {p.ParamName};
        return this;
    }}");
        }

        sb.AppendLine();

        sb.AppendLine($$"""
                          internal override ASSInstance CreateInstance(Player viewer, int id)
                          {
                              {{model.InstanceName}} instance = new {{model.InstanceName}}(viewer, id, this);
                              Instances[viewer] = instance;
                              return instance;
                          }
                      
                          internal override void RemoveInstance(Player viewer) => Instances.Remove(viewer);
                      """);

        sb.AppendLine("}");

        ctx.AddSource($"{model.ClassName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string Camel(string s) => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);

    private static bool IsInstanceHandler(IPropertySymbol prop) => prop.Type is INamedTypeSymbol { IsGenericType: true } named && named.OriginalDefinition.ToDisplayString() == "ASS.Features.Templates.InstanceHandler<T>";

    private static bool IsBool(IPropertySymbol prop) => prop.Type is INamedTypeSymbol named && named.OriginalDefinition.ToDisplayString() == "bool";

    private static bool HasAttributeInHierarchy(INamedTypeSymbol symbol, string ns, string name)
    {
        for (INamedTypeSymbol? namedType = symbol; namedType is not null; namedType = namedType.BaseType)
        {
            foreach (AttributeData attr in namedType.GetAttributes())
            {
                INamedTypeSymbol? attributeClass = attr.AttributeClass;
                if (attributeClass != null
                    && attributeClass.MetadataName == name
                    && attributeClass.ContainingNamespace.ToDisplayString() == ns)
                    return true;
            }
        }
        return false;
    }
}