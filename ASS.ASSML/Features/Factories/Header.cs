namespace ASS.ASSML.Features.Factories
{
    using System.Xml.Linq;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    public static class Header
    {
        internal static IASSObject ParseHeader(XElement el, object? context)
        {
            HeaderTemplate header = HeaderTemplate.Create(el.Key);

            if (el.Attribute("Label") is { } label)
                header.WithLabel(label.Value);

            return header;
        }
    }
}