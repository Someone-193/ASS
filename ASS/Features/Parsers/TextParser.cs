namespace ASS.Features.Parsers
{
    using System.Collections.Generic;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    public delegate bool TryParseDelegate<T>(string str, out T result);

    public class TextParser<T> : ASSParser, IHeaderParser
    {
        private readonly TryParseDelegate<T> parseMethod;

        private readonly TextInputTemplate input = TextInputTemplate.Create("Text Input");

        public TextParser(string key, TryParseDelegate<T> parser)
            : base(key)
        {
            parseMethod = parser;

            input.WithHandler((player, text) =>
            {
                if (IgnoreInitialResponses && text.IsInitialResponse)
                    return;

                OnModified?.Invoke(player);
            });
        }

        public HeaderTemplate Header { get; } = HeaderTemplate.Create("Header");

        public string HeaderLabel
        {
            get;
            set
            {
                field = value;
                Header.Label = value;

                foreach (HeaderInstance instance in Header.Instances.Values)
                    instance.Label = value;
            }
        } = string.Empty;

        public override IEnumerable<IASSObject> GetChildren(Player viewer) => [Header, input];

        public override void Transform(IEnumerable<IParserTransform> transforms)
        {
            foreach (IParserTransform transform in transforms)
            {
                if (transform is HeaderTransformAttribute header)
                    header.Apply(Header);
            }
        }

        public override bool TryParse(Player viewer, out object? value)
        {
            value = null;

            if (!input.Instances.TryGetValue(viewer, out TextInputInstance? instance))
                return false;

            bool success = parseMethod(instance.InputtedText, out T result);

            if (!success)
                return false;

            value = result;
            return true;
        }
    }
}