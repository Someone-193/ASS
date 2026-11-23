namespace ASS.ASSML.Features
{
    using System.Xml.Linq;

    public static class AssmlExtensions
    {
        extension(XElement el)
        {
            internal string Key => el.Attribute("Key")?.Value ?? throw new AssmlException($"<{el.Name.LocalName}> missing Key", el);
        }
    }
}