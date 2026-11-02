namespace ASS.Features.Containers
{
    using System.Collections.Generic;
    using ASS.Features.Interfaces;
    using LabApi.Features.Wrappers;

    public delegate IEnumerable<IASSObject> ASSGenerator(Player player);

    public class TemplateGenerator : IASSContainer
    {
        private TemplateGenerator(string key, ASSGenerator generator)
        {
            Key = key;
            Generator = generator;
        }

        public string Key { get; }

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public ASSGenerator Generator { get; set; }

        public static TemplateGenerator Create(string key, ASSGenerator generator) => new(key, generator);

        public IEnumerable<IASSObject> GetChildren(Player viewer) => Generator(viewer);
    }
}