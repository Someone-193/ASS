namespace ASS.Features.Parsers
{
    using System.Collections.Generic;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    public class BoolParser : ASSParser, IHeaderParser
    {
        public BoolParser(string key)
            : base(key)
        {
            TwoButtons.WithHandler((player, twoButtons) =>
            {
                if (IgnoreInitialResponses && twoButtons.IsInitialResponse)
                    return;

                OnModified?.Invoke(player);
            });
        }

        public HeaderTemplate Header { get; } = HeaderTemplate.Create("Header");

        public TwoButtonsTemplate TwoButtons { get; } = TwoButtonsTemplate
            .Create("BoolParser TwoButtons")
            .WithLabel("Boolean")
            .WithLeftOption("True")
            .WithRightOption("False")
            .WithDefaultRightSelected();

        public override void Transform(IEnumerable<IParserTransform> transforms)
        {
            foreach (IParserTransform transform in transforms)
            {
                if (transform is HeaderTransformAttribute header)
                    header.Apply(Header);

                if (transform is TwoButtonTransformAttribute twoButtons)
                    twoButtons.Apply(TwoButtons);
            }
        }

        public override IEnumerable<IASSObject> GetChildren(Player viewer) => [Header, TwoButtons];

        public override bool TryParse(Player viewer, out object? value)
        {
            value = null;
            if (!TwoButtons.Instances.TryGetValue(viewer, out TwoButtonsInstance instance))
                return false;

            value = instance.LeftSelected;
            return true;
        }
    }
}