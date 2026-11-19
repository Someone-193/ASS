namespace ASS.ASSML.Features
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using ASS.ASSML.Features.Factories;
    using ASS.ASSML.Features.Interfaces;

    using ASS.Features.Interfaces;

    using LabApi.Features.Console;

    public static class AssmlLoader
    {
        private static readonly Dictionary<string, Func<XElement, object?, IASSObject>> Factories = new()
        {
            ["Group"] = Group.ParseGroup,
            ["Header"] = Header.ParseHeader,
            ["Button"] = Button.ParseButton,

            // ButtonPage from ASS.Collections.
        };

        public static void LoadAssml(object obj)
        {
            Type type = obj.GetType();
            string resource = $"{type.Namespace}.{type.Name}.assml";

            using Stream stream = type.Assembly.GetManifestResourceStream(resource) ?? throw new AssmlException($"No .assml resource found for {type.FullName}");

            IASSObject root = Parse(stream, obj);

            // hand parsed tree back to instance via an interface, or store for the partial to read
            (obj as IAssmlObject)?.OnAssmlLoaded(root);
        }

        public static void RegisterFactory(string name, Func<XElement, object?, IASSObject> factory)
        {
            if (Factories.TryAdd(name, factory))
                return;

            Logger.Warn($"Duplicate factory name: '{name}'!");
        }

        public static IASSObject ParseElement(XElement el, object? context = null)
        {
            if (!Factories.TryGetValue(el.Name.LocalName, out Func<XElement, object?, IASSObject>? factory))
                throw new AssmlException($"Unknown element <{el.Name.LocalName}>", el);

            return factory(el, context);
        }

        private static IASSObject Parse(Stream stream, object? context = null)
        {
            XDocument doc = XDocument.Load(stream, LoadOptions.SetLineInfo);
            XElement root = doc.Root!.Elements().Single(); // skip <assml>, take the single top-level object
            return ParseElement(root, context);
        }
    }
}