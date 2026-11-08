namespace ASS.Collections.Features.Assml
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    using ASS.Collections.Features.Pages;
    using ASS.Features.Interfaces;

    // It's either this, or ASS.Collections becomes dependent on ASS.ASSML.
    internal static class AssmlUtils
    {
        private static bool registered;

        static AssmlUtils()
        {
            Assembly? assmlAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == "ASS.ASSML");

            Type? loaderType = assmlAssembly?.GetType("ASS.ASSML.Features.AssmlLoader");

            if (loaderType is null)
                return;

            RegisterMethod = loaderType.GetMethod("RegisterFactory");
            ParseMethod = loaderType.GetMethod("ParseElement");

            Type? extensionsType = assmlAssembly?.GetType("ASS.ASSML.Features.AssmlExtensions");

            if (extensionsType is null)
                return;

            KeyExtension = extensionsType.GetMethod("get_Key", types: [typeof(XElement)]);
        }

        public static MethodInfo? RegisterMethod { get; }

        public static MethodInfo? ParseMethod { get; }

        public static MethodInfo? KeyExtension { get; }

        public static void Register()
        {
            if (registered || RegisterMethod is null || ParseMethod is null || KeyExtension is null)
                return;

            RegisterMethod.Invoke("ButtonPage", [(Func<XElement, object?, IASSObject>)ParseButtonPage]);

            registered = true;
        }

        private static IASSObject ParseButtonPage(XElement el, object? context)
        {
            ButtonPage page = ButtonPage.Create((string)KeyExtension!.Invoke(null, [el]));

            if (el.Attribute("Label") is { } label)
                page.WithLabel(label.Value);

            if (el.Attribute("HoldTime") is { } hold)
                page.WithHoldTime(float.Parse(hold.Value, CultureInfo.InvariantCulture));

            if (el.HasElements)
            {
                foreach (XElement child in el.Elements())
                {
                    page.AddChild((IASSObject)ParseMethod!.Invoke(null, [child, context]));
                }
            }

            return page;
        }
    }
}