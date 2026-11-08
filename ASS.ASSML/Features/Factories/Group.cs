namespace ASS.ASSML.Features.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using ASS.Features.Containers;
    using ASS.Features.Interfaces;

    public static class Group
    {
        private static readonly HashSet<string> Duplicates = [];
        private static readonly HashSet<string> DuplicateCheck = [];

        internal static IASSObject ParseGroup(XElement el, object? context = null)
        {
            IASSObject[] children = el.Elements()
                .Where(e => !e.Name.LocalName.StartsWith("With"))
                .Select(element => AssmlLoader.ParseElement(element, context))
                .ToArray();

            Duplicates.Clear();
            DuplicateCheck.Clear();
            foreach (IASSObject child in children)
            {
                if (!DuplicateCheck.Add(child.Key))
                    Duplicates.Add(child.Key);
            }

            if (Duplicates.Count > 0)
                throw new AssmlException($"Duplicate keys: {string.Join(", ", Duplicates)}", el);

            return ASSGroup.Create(el.Key).WithChildren(children);
        }
    }
}