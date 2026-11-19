using Microsoft.CodeAnalysis;

namespace ASS.Generators
{
    public static class Helpers
    {
        public static string Camel(string s) => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);

        public static bool IsInstanceHandler(IPropertySymbol prop) => prop.Type is INamedTypeSymbol { IsGenericType: true } named && named.OriginalDefinition.ToDisplayString() == "ASS.Features.Templates.InstanceHandler<T>";

        public static bool IsBool(IPropertySymbol prop) => prop.Type is INamedTypeSymbol named && named.OriginalDefinition.ToDisplayString() == "bool";

        public static bool HasAttributeInHierarchy(INamedTypeSymbol symbol, string ns, string name)
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

        public static bool HasAttribute(ISymbol symbol, string ns, string name, out AttributeData attribute)
        {
            foreach (AttributeData attr in symbol.GetAttributes())
            {
                INamedTypeSymbol? attributeClass = attr.AttributeClass;
                if (attributeClass != null
                    && attributeClass.MetadataName == name
                    && attributeClass.ContainingNamespace.ToDisplayString() == ns)
                {
                    attribute = attr;
                    return true;
                }
            }

            attribute = null!;
            return false;
        }
    }
}