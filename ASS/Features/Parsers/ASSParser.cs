namespace ASS.Features.Parsers
{
    using System;
    using System.Collections.Generic;

    using ASS.Features.Interfaces;

    using LabApi.Features.Wrappers;

    public abstract class ASSParser : IASSContainer
    {
        protected ASSParser(string key) => Key = key;

        public string Key { get; }

        public Action<Player>? OnModified { get; set; }

        public bool IgnoreInitialResponses { get; set; } = true;

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public abstract void Transform(IEnumerable<IParserTransform> transforms);

        public abstract IEnumerable<IASSObject> GetChildren(Player viewer);

        public abstract bool TryParse(Player viewer, out object? value);
    }
}